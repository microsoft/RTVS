#include "stdafx.h"
#include "log.h"
#include "xamlbuilder.h"
#include "Rgraphicsapi.h"
#include "host.h"
#include "msvcrt.h"

using namespace rhost::log;


typedef struct {
    double width;
    double height;
    bool debug;
    bool write_on_mode;
    rhost::graphics::xaml_builder* xaml;
} XamlDeviceDesc;
typedef XamlDeviceDesc* pXamlDeviceDesc;

// TODO: decide how we determine size and dpi.
// width and height (in inches) are configurable
// from user's code so we can experiment for now.

#define DEVICE_UNIT_TO_INCH(x) (x / 72)
#define INCH_TO_DEVICE_UNIT(x) (72.0 * x)

#define TRACING(dd) (((pXamlDeviceDesc)dd->deviceSpecific)->debug)

#define FONTFACE_PLAIN          1
#define FONTFACE_BOLD           2
#define FONTFACE_ITALIC         3
#define FONTFACE_BOLD_ITALIC    4

#define DEFAULT_FONT_NAME       "Arial"


static double string_width(const char *str, double ps, int fontface) {
    SIZE size;
    HDC dc = GetDC(NULL);
    // https://msdn.microsoft.com/en-us/library/windows/desktop/dd183499(v=vs.85).aspx
    HFONT font = CreateFontA((int)ps, 0, 0, 0,
        (fontface == FONTFACE_BOLD || fontface == FONTFACE_BOLD_ITALIC) ? FW_BOLD : FW_NORMAL,
        fontface == FONTFACE_ITALIC || fontface == FONTFACE_BOLD_ITALIC,
        FALSE,
        FALSE,
        DEFAULT_CHARSET, OUT_DEFAULT_PRECIS,
        CLIP_DEFAULT_PRECIS, DEFAULT_QUALITY,
        FF_DONTCARE | DEFAULT_PITCH,
        DEFAULT_FONT_NAME);
    auto old_font = SelectObject(dc, font);
    BOOL result = GetTextExtentPoint32A(dc, str, (int)strlen(str), &size);
    SelectObject(dc, old_font);
    DeleteObject(font);
    ReleaseDC(NULL, dc);
    if (result) {
        return size.cx;
    }
    else {
        return 0;
    }
}

static std::string get_temp_file_path() {
    // TODO: do we need to use unicode version?
    char folderpath[1024];
    char filepath[1024];
    GetTempPathA(1024, folderpath);
    GetTempFileNameA(folderpath, "rt", 0, filepath);

    return std::string(filepath);
}

static void write_xaml(pDevDesc dd) {
    pXamlDeviceDesc xdd = (pXamlDeviceDesc)dd->deviceSpecific;

    auto path = get_temp_file_path();
    xdd->xaml->write_xaml(path);
    // TODO: also keep track of the external bitmap files we create so we can delete them
    rhost::host::plot_xaml(path);
}

static std::string r_fontface_to_xaml_font_weight(int fontface) {
    switch (fontface) {
    case FONTFACE_BOLD:
    case FONTFACE_BOLD_ITALIC:
        return "Bold";
    default:
        return "";
    }
}

static std::string r_fontface_to_xaml_font_style(int fontface) {
    switch (fontface) {
    case FONTFACE_ITALIC:
    case FONTFACE_BOLD_ITALIC:
        return "Italic";
    default:
        return "";
    }
}

static std::string r_color_to_xaml(int col) {
    if (col == NA_INTEGER) {
        return "";
    }
    char buffer[100];
    sprintf(buffer, "#%02X%02X%02X%02X", R_ALPHA(col), R_RED(col), R_GREEN(col), R_BLUE(col));
    return std::string(buffer);
}

static std::string r_line_type_to_xaml(int lty) {
    // Integer is a list of (up to 8) 4-bit values, starting the least significant bit.
    // Values represent the length of each segment, alternating between ON and OFF.
    // See help(par) for more details on line types.
    //
    // Examples for predefined styles:
    // LTY_SOLID     0x00000000
    // LTY_DASHED    0x00000044
    // LTY_DOTTED    0x00000031
    // LTY_DOTDASH   0x00003431
    // LTY_LONGDASH  0x00000037
    // LTY_TWODASH   0x00002622
    if (lty == 0) {
        return "";
    }

    std::ostringstream stream;

    for (int i = 0; i < 8; i++) {
        int val = (lty >> (i * 4)) & 0xf;
        if (val == 0) {
            break;
        }
        stream << val << " ";
    }

    std::string str = stream.str();
    boost::algorithm::trim_right(str);
    return str;
}

static std::string r_line_join_to_xaml(int ljoin) {
    switch (ljoin) {
    case GE_ROUND_JOIN:
        return "Round";
    case GE_MITRE_JOIN:
        return "Miter";
    case GE_BEVEL_JOIN:
        return "Bevel";
    default:
        return "";
    }
}

static std::string r_line_end_to_xaml(int lend) {
    switch (lend) {
    case GE_ROUND_CAP:
        return "Round";
    case GE_BUTT_CAP:
    case GE_SQUARE_CAP:
        return "Square";
    default:
        return "";
    }
}

static void write_bitmap(std::ofstream& f, unsigned int *raster, int w, int h) {
    BITMAPV4HEADER infoHeader;
    memset(&infoHeader, 0, sizeof(infoHeader));
    infoHeader.bV4Size = sizeof(infoHeader);
    infoHeader.bV4Width = w;
    infoHeader.bV4Height = h;
    infoHeader.bV4Planes = 0;
    infoHeader.bV4BitCount = 32;
    infoHeader.bV4V4Compression = BI_BITFIELDS;
    infoHeader.bV4SizeImage = 4 * w * h;
    infoHeader.bV4XPelsPerMeter = 2835;
    infoHeader.bV4YPelsPerMeter = 2835;
    infoHeader.bV4ClrUsed = 0;
    infoHeader.bV4ClrImportant = 0;
    infoHeader.bV4BlueMask = 0x00ff0000;
    infoHeader.bV4GreenMask = 0x0000ff00;
    infoHeader.bV4RedMask = 0x000000ff;
    infoHeader.bV4AlphaMask = 0xff000000;
    infoHeader.bV4CSType = 0x57696e20;

    BITMAPFILEHEADER bmp;
    memset(&bmp, 0, sizeof(bmp));
    bmp.bfType = 0x4D42;
    bmp.bfSize = sizeof(bmp) + sizeof(infoHeader) + 4 * w * h;
    bmp.bfOffBits = sizeof(bmp) + sizeof(infoHeader);

    f.write((char*)&bmp, sizeof(bmp));
    f.write((char*)&infoHeader, sizeof(infoHeader));
    f.write((char*)raster, 4 * w * h);
}

extern "C" void VSGD_Activate(pDevDesc dd) {
    if (TRACING(dd)) {
        logf("VSGD_Activate\n");
    }
}

extern "C" void VSGD_Circle(double x, double y, double r, pGEcontext gc, pDevDesc dd) {
    if (TRACING(dd)) {
        logf("VSGD_Circle(x=%f,y=%f,r=%f)\n", x, y, r);
    }

    double top = y - r;
    double left = x - r;
    double width = r * 2;
    double height = r * 2;

    pXamlDeviceDesc xdd = (pXamlDeviceDesc)dd->deviceSpecific;
    xdd->xaml->circle(top, left, width, height, r_color_to_xaml(gc->fill), r_color_to_xaml(gc->col), gc->lwd,
        r_line_type_to_xaml(gc->lty),
        r_line_join_to_xaml(gc->ljoin),
        r_line_end_to_xaml(gc->lend),
        gc->lmitre);
}

extern "C" void VSGD_Clip(double x0, double x1, double y0, double y1, pDevDesc dd) {
    if (TRACING(dd)) {
        logf("VSGD_Clip(x0=%f,x1=%f,y0=%f,y1=%f)\n", x0, x1, y0, y1);
    }

    pXamlDeviceDesc xdd = (pXamlDeviceDesc)dd->deviceSpecific;
    xdd->xaml->clip_begin(x0, x1, y0, y1);
}

extern "C" void VSGD_Close(pDevDesc dd) {
    if (TRACING(dd)) {
        logf("VSGD_Close\n");
    }

    pXamlDeviceDesc xdd = (pXamlDeviceDesc)dd->deviceSpecific;
    // TODO: figure out the right time to send the plot
    if (!xdd->write_on_mode) {
        xdd->xaml->clip_end();
        // TODO: if we're being closed because the r.host process is exiting, 
        // this will cause an access violation in websocket code
        write_xaml(dd);
    }

    delete xdd->xaml;
    delete xdd;
    dd->deviceSpecific = NULL;
}

extern "C" void VSGD_Deactivate(pDevDesc dd) {
    if (TRACING(dd)) {
        logf("VSGD_Deactivate\n");
    }
}

extern "C"  Rboolean VSGD_Locator(double *x, double *y, pDevDesc dd) {
    if (TRACING(dd)) {
        logf("VSGD_Locator\n");
    }

    *x = 0;
    *y = 0;

    return R_FALSE;
}

extern "C" void VSGD_Line(double x1, double y1, double x2, double y2, const pGEcontext gc, pDevDesc dd) {
    if (TRACING(dd)) {
        logf("VSGD_Line(x1=%f,y1=%f,x2=%f,y2=%f)\n", x1, y1, x2, y2);
    }

    pXamlDeviceDesc xdd = (pXamlDeviceDesc)dd->deviceSpecific;
    xdd->xaml->line(x1, y1, x2, y2, r_color_to_xaml(gc->col), gc->lwd,
        r_line_type_to_xaml(gc->lty),
        r_line_join_to_xaml(gc->ljoin),
        r_line_end_to_xaml(gc->lend),
        gc->lmitre);
}

extern "C" void VSGD_MetricInfo(int c, const pGEcontext gc, double* ascent, double* descent, double* width, pDevDesc dd) {
    if (TRACING(dd)) {
        logf("VSGD_MetricInfo(c=%d)\n", c);
    }

    *ascent = 0;
    *descent = 0;
    *width = 0;

    if (TRACING(dd)) {
        logf("  {ascent=%f,descent=%f,width=%f}\n", *ascent, *descent, *width);
    }
}

extern "C" void VSGD_Mode(int mode, pDevDesc dd) {
    if (TRACING(dd)) {
        logf("VSGD_Mode(mode=%d)\n", mode);
    }

    if (mode == 0) {
        pXamlDeviceDesc xdd = (pXamlDeviceDesc)dd->deviceSpecific;
        xdd->xaml->clip_end();
        // TODO: figure out the right time to send the plot
        if (xdd->write_on_mode) {
            write_xaml(dd);
        }
    }
}

extern "C" void VSGD_NewPage(const pGEcontext gc, pDevDesc dd) {
    if (TRACING(dd)) {
        logf("VSGD_NewPage\n");
    }

    pXamlDeviceDesc xdd = (pXamlDeviceDesc)dd->deviceSpecific;
    xdd->xaml->clear();
}

extern "C" void VSGD_Polygon(int n, double *x, double *y, const pGEcontext gc, pDevDesc dd) {
    if (TRACING(dd)) {
        logf("VSGD_Polygon([");
        for (int i = 0; i < n; i++) {
            logf("(%f,%f)", x[i], y[i]);
        }
        logf("])\n");
    }

    pXamlDeviceDesc xdd = (pXamlDeviceDesc)dd->deviceSpecific;
    xdd->xaml->polygon(n, x, y, r_color_to_xaml(gc->fill), r_color_to_xaml(gc->col), gc->lwd,
        r_line_type_to_xaml(gc->lty),
        r_line_join_to_xaml(gc->ljoin),
        r_line_end_to_xaml(gc->lend),
        gc->lmitre);
}

extern "C" void VSGD_Polyline(int n, double *x, double *y, const pGEcontext gc, pDevDesc dd) {
    if (TRACING(dd)) {
        logf("VSGD_Polyline([");
        for (int i = 0; i < n; i++) {
            logf("(%f,%f)", x[i], y[i]);
        }
        logf("])\n");
    }

    pXamlDeviceDesc xdd = (pXamlDeviceDesc)dd->deviceSpecific;
    xdd->xaml->polyline(n, x, y, r_color_to_xaml(gc->col), gc->lwd,
        r_line_type_to_xaml(gc->lty),
        r_line_join_to_xaml(gc->ljoin),
        r_line_end_to_xaml(gc->lend),
        gc->lmitre);
}

extern "C" void VSGD_Rect(double x0, double y0, double x1, double y1, const pGEcontext gc, pDevDesc dd) {
    if (TRACING(dd)) {
        logf("VSGD_Rect(x0=%f,y0=%f,x1=%f,y1=%f\n", x0, y0, x1, y1);
    }

    double left = fmin(x0, x1);
    double top = fmin(y0, y1);
    double width = fabs(x1 - x0);
    double height = fabs(y1 - y0);

    pXamlDeviceDesc xdd = (pXamlDeviceDesc)dd->deviceSpecific;
    xdd->xaml->rect(top, left, width, height, r_color_to_xaml(gc->fill), r_color_to_xaml(gc->col), gc->lwd,
        r_line_type_to_xaml(gc->lty),
        r_line_join_to_xaml(gc->ljoin),
        r_line_end_to_xaml(gc->lend),
        gc->lmitre);
}

extern "C" void VSGD_Path(double *x, double *y, int npoly, int *nper, Rboolean winding, const pGEcontext gc, pDevDesc dd) {
    if (TRACING(dd)) {
        logf("VSGD_Path(xy=[");
        int index = 0;
        for (int i = 0; i < npoly; i++) {
            int points = nper[i];
            logf("{");
            for (int j = 0; j < points; j++) {
                logf("(%f,%f)", x[index], y[index]);
                index++;
            }
            logf("}");
        }
        logf("],winding=%d)\n", winding);
    }

    pXamlDeviceDesc xdd = (pXamlDeviceDesc)dd->deviceSpecific;
    xdd->xaml->path(x, y, npoly, nper, winding != R_FALSE, r_color_to_xaml(gc->fill), r_color_to_xaml(gc->col), gc->lwd,
        r_line_type_to_xaml(gc->lty),
        r_line_join_to_xaml(gc->ljoin),
        r_line_end_to_xaml(gc->lend),
        gc->lmitre);
}

extern "C" void VSGD_Raster(unsigned int *raster, int w, int h, double x, double y, double width, double height, double rot, Rboolean interpolate, const pGEcontext gc, pDevDesc dd) {
    if (TRACING(dd)) {
        logf("VSGD_Raster(w=%d,h=%d,x=%f,y=%f,width=%f,height=%f,rot=%f,interpolate=%d)\n", w, h, x, y, width, height, rot, interpolate);
    }

    pXamlDeviceDesc xdd = (pXamlDeviceDesc)dd->deviceSpecific;

    auto path = get_temp_file_path();
    double left = x;
    double top = xdd->height - y;

    std::ofstream f(path, std::ofstream::out);
    write_bitmap(f, raster, w, h);
    f.close();

    xdd->xaml->bitmap_external_file(top, left, width, 0.0 - height, 0.0 - rot, interpolate != R_FALSE, path);
}

extern "C" SEXP VSGD_Cap(pDevDesc dd) {
    if (TRACING(dd)) {
        logf("VSGD_Cap()\n");
    }

    return R_NilValue;
}

extern "C" void VSGD_Size(double *left, double *right, double *bottom, double *top, pDevDesc dd) {
    if (TRACING(dd)) {
        logf("VSGD_Size()\n");
    }

    *left = dd->left;
    *right = dd->right;
    *bottom = dd->bottom;
    *top = dd->top;
}

extern "C" double VSGD_StrWidth(const char *str, const pGEcontext gc, pDevDesc dd) {
    if (TRACING(dd)) {
        logf("VSGD_StrWidth(str='%s')\n", str);
        logf("  <fontface=%d,fontfamily='%s',ps='%f',cex='%f'>\n", gc->fontface, gc->fontfamily, gc->ps, gc->cex);
    }

    double width = string_width(str, gc->ps * gc->cex, gc->fontface);

    if (TRACING(dd)) {
        logf("  {return=%f}\n", width);
    }

    return width;
}

extern "C" void VSGD_Text(double x, double y, const char *str, double rot, double hadj, const pGEcontext gc, pDevDesc dd) {
    if (TRACING(dd)) {
        logf("VSGD_Text(x=%f,y=%f,str='%s',rot=%f,hadj=%f\n", x, y, str, rot, hadj);
        logf("  <fontface=%d,fontfamily='%s',ps='%f',cex='%f',col='%d'>\n", gc->fontface, gc->fontfamily, gc->ps, gc->cex, gc->col);
    }

    y -= gc->ps * gc->cex;

    pXamlDeviceDesc xdd = (pXamlDeviceDesc)dd->deviceSpecific;
    xdd->xaml->text(x, y, str, 0.0 - rot, hadj,
        r_color_to_xaml(gc->col), gc->ps * gc->cex,
        r_fontface_to_xaml_font_weight(gc->fontface),
        r_fontface_to_xaml_font_style(gc->fontface));
}

extern "C" void VSGD_OnExit(pDevDesc dd) {
    if (TRACING(dd)) {
        logf("VSGD_OnExit\n");
    }
}

extern "C"  Rboolean VSGD_NewFrameConfirm(pDevDesc dd) {
    if (TRACING(dd)) {
        logf("VSGD_NewFrameConfirm\n");
    }

    return R_FALSE;
}

extern "C"  void VSGD_TextUTF8(double x, double y, const char *str, double rot, double hadj, const pGEcontext gc, pDevDesc dd) {
    if (TRACING(dd)) {
        logf("VSGD_TextUTF8(x=%f,y=%f,str='%s',rot=%f,hadj=%f\n", x, y, str, rot, hadj);
        logf("  <fontface=%d,fontfamily='%s',ps='%f',cex='%f',col='%d'>\n", gc->fontface, gc->fontfamily, gc->ps, gc->cex, gc->col);
    }

    y -= gc->ps * gc->cex;

    pXamlDeviceDesc xdd = (pXamlDeviceDesc)dd->deviceSpecific;
    xdd->xaml->text(x, y, str, 0.0 - rot, hadj,
        r_color_to_xaml(gc->col), gc->ps * gc->cex,
        r_fontface_to_xaml_font_weight(gc->fontface),
        r_fontface_to_xaml_font_style(gc->fontface));
}

extern "C"  double VSGD_StrWidthUTF8(const char *str, const pGEcontext gc, pDevDesc dd) {
    if (TRACING(dd)) {
        logf("VSGD_StrWidthUTF8(str='%s')\n", str);
        logf("  <fontface=%d,fontfamily='%s',ps='%f',cex='%f'>\n", gc->fontface, gc->fontfamily, gc->ps, gc->cex);
    }

    double width = string_width(str, gc->ps * gc->cex, gc->fontface);

    if (TRACING(dd)) {
        logf("  {return=%f}\n", width);
    }

    return width;
}

extern "C"  void VSGD_EventHelper(pDevDesc dd, int code) {
    if (TRACING(dd)) {
        logf("VSGD_EventHelper(code=%d)\n", code);
    }

    // TODO: figure out the right time to send the plot
    //if (code == 1) {
    //    write_xaml(dd);
    //}
}

extern "C"  int VSGD_Holdflush(pDevDesc dd, int level) {
    if (TRACING(dd)) {
        logf("VSGD_Holdflush(level=%d)\n", level);
    }

    // TODO: figure out the right time to send the plot
    //write_xaml(dd);

    return 0;
}

static pDevDesc new_devdesc(double width, double height) {
    pDevDesc dd = (pDevDesc)rhost::msvcrt::malloc(sizeof(DevDesc));
    if (!dd)
    {
        return 0;
    }
    memset(dd, 0, sizeof(DevDesc));

    // TODO: if this fails, need to free dd
    pXamlDeviceDesc xdd = new XamlDeviceDesc();

    int startfill = R_RGB(255, 255, 255);

    xdd->width = INCH_TO_DEVICE_UNIT(width);
    xdd->height = INCH_TO_DEVICE_UNIT(height);
    xdd->debug = false;
    xdd->write_on_mode = true;
    xdd->xaml = new rhost::graphics::xaml_builder(xdd->width, xdd->height, r_color_to_xaml(startfill), DEFAULT_FONT_NAME);

    dd->left = 0;
    dd->right = xdd->width;
    dd->bottom = xdd->height;
    dd->top = 0;

    dd->clipLeft = dd->left;
    dd->clipRight = dd->right;
    dd->clipBottom = dd->bottom;
    dd->clipTop = dd->top;

    dd->xCharOffset = 0;
    dd->yCharOffset = 0;
    dd->yLineBias = 0;

    dd->ipr[0] = dd->ipr[1] = DEVICE_UNIT_TO_INCH(1.0);
    dd->cra[0] = dd->cra[1] = 10;
    dd->gamma = 0;

    dd->canClip = R_FALSE;
    dd->canChangeGamma = R_FALSE;
    dd->canHAdj = 0;

    dd->startps = 10;
    dd->startcol = R_RGB(0, 0, 0);
    dd->startfill = startfill;
    dd->startlty = LTY_SOLID;
    dd->startfont = 0;
    dd->startgamma = 0;

    dd->deviceSpecific = xdd;

    dd->displayListOn = R_FALSE;
    dd->canGenMouseDown = R_FALSE;
    dd->canGenMouseMove = R_FALSE;
    dd->canGenMouseUp = R_FALSE;
    dd->canGenKeybd = R_FALSE;

    dd->activate = VSGD_Activate;
    dd->circle = VSGD_Circle;
    dd->clip = VSGD_Clip;
    dd->close = VSGD_Close;
    dd->deactivate = VSGD_Deactivate;
    dd->locator = VSGD_Locator;
    dd->line = VSGD_Line;
    dd->metricInfo = VSGD_MetricInfo;
    dd->mode = VSGD_Mode;
    dd->newPage = VSGD_NewPage;
    dd->polygon = VSGD_Polygon;
    dd->polyline = VSGD_Polyline;
    dd->rect = VSGD_Rect;
    dd->path = VSGD_Path;
    dd->raster = VSGD_Raster;
    dd->cap = VSGD_Cap;
    dd->size = VSGD_Size;
    dd->strWidth = VSGD_StrWidth;
    dd->text = VSGD_Text;
    dd->onExit = VSGD_OnExit;
    dd->newFrameConfirm = VSGD_NewFrameConfirm;
    dd->textUTF8 = VSGD_TextUTF8;
    dd->strWidthUTF8 = VSGD_StrWidthUTF8;
    dd->eventHelper = VSGD_EventHelper;
    dd->holdflush = VSGD_Holdflush;

    dd->hasTextUTF8 = R_FALSE;
    dd->wantSymbolUTF8 = R_FALSE;
    dd->useRotatedTextInContour = R_FALSE;

    dd->haveTransparency = 1; //no
    dd->haveTransparentBg = 2; //fully
    dd->haveRaster = 2; //yes
    dd->haveCapture = 1; //no
    dd->haveLocator = 1; //no

    return dd;
}

extern "C" SEXP C_vsgd(SEXP args) {
    args = CDR(args);
    SEXP width = CAR(args);
    args = CDR(args);
    SEXP height = CAR(args);

    double *w = REAL(width);
    double *h = REAL(height);

    R_GE_checkVersionOrDie(R_GE_version);

    R_CheckDeviceAvailable();
    BEGIN_SUSPEND_INTERRUPTS{
        pDevDesc dd = new_devdesc(*w, *h);
        pGEDevDesc gdd = GEcreateDevDesc(dd);
        GEaddDevice2(gdd, "vsgd");
    } END_SUSPEND_INTERRUPTS;

    return R_NilValue;
}

static R_ExternalMethodDef external_methods[] =
{
    { "C_vsgd", (DL_FUNC)&C_vsgd, 2 },
    { NULL, NULL, 0 }
};

extern "C" void R_init_vsgd(DllInfo *dll)
{
    R_registerRoutines(dll, NULL, NULL, NULL, external_methods);
}
