#pragma once

#include <list>
#include <fstream>
#include <sstream>
#include <string>

#include "boost/algorithm/string/replace.hpp"

#define XAML_XMLNS "http://schemas.microsoft.com/winfx/2006/xaml/presentation"

// Re-enable this code if we want to use base-64 encoded bitmaps (with appropriate namespace/assembly name)
//#define VS_XMLNS "clr-namespace:Microsoft.VisualStudioTools.Wpf;assembly=XamlViewer"

namespace rhost {
    namespace graphics {
        class xaml_builder {

        private:
            std::list<std::string> _xaml;
            double _width;
            double _height;
            std::string _background_color;
            std::string _font_family;
            bool _clipping;

        public:
            xaml_builder(double width, double height, std::string background_color, std::string font_family) {
                _width = width;
                _height = height;
                _background_color = background_color;
                _font_family = font_family;
                _clipping = false;
            }

            void line(double x1, double y1, double x2, double y2, std::string stroke_color, double stroke_thickness, const std::string& line_dash, const std::string& line_join, const std::string& line_cap, double miter_limit) {
                std::ostringstream stream;

                stream << "<Line";
                write_attr("X1", x1, stream);
                write_attr("Y1", y1, stream);
                write_attr("X2", x2, stream);
                write_attr("Y2", y2, stream);
                write_stroke(stroke_color, stroke_thickness, line_dash, line_join, line_cap, miter_limit, stream);
                stream << " />";

                _xaml.push_back(stream.str());
            }

            void circle(double top, double left, double width, double height, const std::string& fill_color, const std::string& stroke_color, double stroke_thickness, const std::string& line_dash, const std::string& line_join, const std::string& line_cap, double miter_limit) {
                std::ostringstream stream;

                stream << "<Ellipse";
                write_attr("Canvas.Left", left, stream);
                write_attr("Canvas.Top", top, stream);
                write_attr("Width", width, stream);
                write_attr("Height", height, stream);
                write_stroke(stroke_color, stroke_thickness, line_dash, line_join, line_cap, miter_limit, stream);
                if (!fill_color.empty()) {
                    write_attr("Fill", fill_color, stream);
                }
                stream << " />";

                _xaml.push_back(stream.str());
            }

            void polygon(int n, double *x, double *y, const std::string& fill_color, const std::string& stroke_color, double stroke_thickness, const std::string& line_dash, const std::string& line_join, const std::string& line_cap, double miter_limit) {
                std::ostringstream stream;

                stream << "<Polygon Points=\"";
                write_points(n, x, y, stream);
                stream << "\"";
                write_stroke(stroke_color, stroke_thickness, line_dash, line_join, line_cap, miter_limit, stream);
                if (!fill_color.empty()) {
                    write_attr("Fill", fill_color, stream);
                }
                stream << " />";

                _xaml.push_back(stream.str());
            }

            void polyline(int n, double *x, double *y, const std::string& stroke_color, double stroke_thickness, const std::string& line_dash, const std::string& line_join, const std::string& line_cap, double miter_limit) {
                std::ostringstream stream;

                stream << "<Polyline Points=\"";
                write_points(n, x, y, stream);
                stream << "\"";
                write_stroke(stroke_color, stroke_thickness, line_dash, line_join, line_cap, miter_limit, stream);
                stream << " />";

                _xaml.push_back(stream.str());
            }

            void rect(double top, double left, double width, double height, const std::string& fill_color, const std::string& stroke_color, double stroke_thickness, const std::string& line_dash, const std::string& line_join, const std::string& line_cap, double miter_limit) {
                std::ostringstream stream;

                stream << "<Rectangle";
                write_attr("Canvas.Left", left, stream);
                write_attr("Canvas.Top", top, stream);
                write_attr("Width", width, stream);
                write_attr("Height", height, stream);
                write_stroke(stroke_color, stroke_thickness, line_dash, line_join, line_cap, miter_limit, stream);
                if (!fill_color.empty()) {
                    write_attr("Fill", fill_color, stream);
                }
                stream << " />";

                _xaml.push_back(stream.str());
            }

            void path(double *x, double *y, int npoly, int *nper, bool winding, const std::string& fill_color, const std::string& stroke_color, double stroke_thickness, const std::string& line_dash, const std::string& line_join, const std::string& line_cap, double miter_limit) {
                std::ostringstream stream;

                // Path Markup Syntax
                // https://msdn.microsoft.com/en-us/library/vstudio/ms752293(v=vs.100).aspx
                stream << "<Path Data=\"";
                if (winding) {
                    // Nonzero
                    stream << "F 1 ";
                } else {
                    // EvenOdd
                    stream << "F 0 ";
                }
                int index = 0;
                for (int i = 0; i < npoly; i++) {
                    int points = nper[i];
                    stream << "M " << x[index] << "," << y[index] << " ";
                    index++;
                    for (int j = 1; j < points; j++) {
                        stream << "L " << x[index] << "," << y[index] << " ";
                        index++;
                    }
                    stream << "Z ";
                }
                stream << "\"";
                write_stroke(stroke_color, stroke_thickness, line_dash, line_join, line_cap, miter_limit, stream);
                if (!fill_color.empty()) {
                    write_attr("Fill", fill_color, stream);
                }
                stream << " />";

                _xaml.push_back(stream.str());
            }

            // TODO: currently unused, not sure if we'll end up using it or not.  keeping in case we do.
            //void bitmap_embedded_base64(double top, double left, double width, double height, double rotation, bool interpolate, const std::string& base64_encoded_data) {
            //    std::ostringstream stream;

            //    stream << "<Image";
            //    write_attr("Canvas.Left", left, stream);
            //    write_attr("Canvas.Top", top, stream);
            //    write_attr("Width", width, stream);
            //    write_attr("Height", height, stream);
            //    if (!interpolate) {
            //        stream << " RenderOptions.BitmapScalingMode=\"NearestNeighbor\"";
            //    }
            //    stream << " >";
            //    if (rotation != 0) {
            //        stream << "<Image.RenderTransform>";
            //        stream << "<TransformGroup>";
            //        stream << "<RotateTransform Angle=\"" << rotation << "\" />";
            //        stream << "</TransformGroup>";
            //        stream << "</Image.RenderTransform>";
            //    }
            //    stream << "<Image.Source>";
            //    stream << "<vs:Base64ImageSource>";
            //    stream << "<vs:Base64ImageSource.Base64>";
            //    stream << base64_encoded_data;
            //    stream << "</vs:Base64ImageSource.Base64>";
            //    stream << "</vs:Base64ImageSource>";
            //    stream << "</Image.Source>";
            //    stream << "</Image>";

            //    _xaml.push_back(stream.str());
            //}

            void bitmap_external_file(double top, double left, double width, double height, double rotation, bool interpolate, const std::string& filepath) {
                std::ostringstream stream;

                stream << "<Image";
                write_attr("Canvas.Left", left, stream);
                write_attr("Canvas.Top", top, stream);
                write_attr("Width", width, stream);
                write_attr("Height", height, stream);
                write_attr("Source", filepath, stream);
                if (!interpolate) {
                    write_attr("RenderOptions.BitmapScalingMode", "NearestNeighbor", stream);
                }
                if (rotation != 0) {
                    stream << " >";
                    stream << "<Image.RenderTransform>";
                    stream << "<TransformGroup>";
                    stream << "<RotateTransform Angle=\"" << rotation << "\" />";
                    stream << "</TransformGroup>";
                    stream << "</Image.RenderTransform>";
                    stream << "</Image>";
                } else {
                    stream << " />";
                }

                _xaml.push_back(stream.str());
            }

            void text(double x, double y, const std::string& str, double rotation, double hadj, const std::string& color, double font_size, const std::string& font_weight, const std::string& font_style) {
                std::ostringstream stream;

                stream << "<TextBlock";
                write_attr("RenderTransformOrigin", "0, 1", stream);
                write_attr("Canvas.Left", x, stream);
                write_attr("Canvas.Top", y, stream);
                write_attr("Text", xml_escape(str), stream);
                write_attr("FontFamily", _font_family, stream);
                write_attr("FontSize", font_size, stream);
                write_attr("Foreground", color, stream);
                if (!font_weight.empty()) {
                    write_attr("FontWeight", font_weight, stream);
                }
                if (!font_style.empty()) {
                    write_attr("FontStyle", font_style, stream);
                }
                if (rotation != 0) {
                    stream << ">";
                    stream << "<TextBlock.RenderTransform>";
                    stream << "<TransformGroup>";
                    stream << "<RotateTransform Angle=\"" << rotation << "\" />";
                    stream << "</TransformGroup>";
                    stream << "</TextBlock.RenderTransform>";
                    stream << "</TextBlock>";
                }
                else {
                    stream << " />";
                }

                _xaml.push_back(stream.str());
            }

            void clip_begin(double x0, double x1, double y0, double y1) {
                if (_clipping) {
                    _xaml.push_back("</Canvas>");
                }

                _clipping = true;

                std::ostringstream stream;
                stream << "<Canvas><Canvas.Clip><RectangleGeometry Rect=\"" << x0 << ", " << y1 << ", " << x1 << ", " << y0 << "\"/></Canvas.Clip>";

                _xaml.push_back(stream.str());
            }

            void clip_end() {
                if (_clipping) {
                    _clipping = false;
                    _xaml.push_back("</Canvas>");
                }
            }

            void write_xaml(const std::string& filepath) {
                std::ofstream filestream(filepath);
                write_xaml(filestream);
                filestream.close();
            }

            void write_xaml(std::ofstream& f) {
                f << "<Canvas";
                write_attr("xmlns", "http://schemas.microsoft.com/winfx/2006/xaml/presentation", f);
                // Re-enable this code if we want to use base-64 encoded bitmaps
                //write_attr("xmlns:vs", VS_XMLNS, f);
                write_attr("Height", _height, f);
                write_attr("Width", _width, f);
                write_attr("Background", _background_color, f);
                f << ">\r\n";
                for (std::list<std::string>::iterator it = _xaml.begin(); it != _xaml.end(); it++) {
                    f << *it << "\r\n";
                }
                f << "</Canvas>";
            }

            void clear() {
                _xaml.clear();
                _clipping = false;
            }

        private:
            template <typename T>
            static void write_attr(const char * name, T val, std::ostringstream& stream) {
                stream << " ";
                stream << name;
                stream << "=\"";
                stream << val;
                stream << "\"";
            }

            template <typename T>
            static void write_attr(const char * name, T val, std::ofstream& stream) {
                stream << " ";
                stream << name;
                stream << "=\"";
                stream << val;
                stream << "\"";
            }

            static void write_points(int n, double *x, double *y, std::ostringstream& stream) {
                for (int j = 0; j < n; j++) {
                    if (j > 0) {
                        stream << " ";
                    }
                    stream << x[j] << "," << y[j];
                }
            }

            static void write_stroke(const std::string& stroke_color, double stroke_thickness, const std::string& line_dash, const std::string& line_join, const std::string& line_cap, double miter_limit, std::ostringstream& stream) {
                write_attr("StrokeThickness", stroke_thickness, stream);
                if (!stroke_color.empty()) {
                    write_attr("Stroke", stroke_color, stream);
                }
                if (!line_dash.empty()) {
                    write_attr("StrokeDashArray", line_dash, stream);
                }
                if (!line_join.empty()) {
                    write_attr("StrokeLineJoin", line_join, stream);
                    write_attr("StrokeMiterLimit", miter_limit, stream);
                }
                if (!line_cap.empty()) {
                    write_attr("StrokeStartLineCap", line_cap, stream);
                    write_attr("StrokeEndLineCap", line_cap, stream);
                    if (!line_dash.empty()) {
                        write_attr("StrokeDashCap", line_cap, stream);
                    }
                }
            }

            static std::string xml_escape(const std::string& text) {
                std::string escaped(text);
                boost::algorithm::replace_all(escaped, "&", "&amp;");
                boost::algorithm::replace_all(escaped, "\"", "&quot;");
                boost::algorithm::replace_all(escaped, ">", "&gt;");
                boost::algorithm::replace_all(escaped, "<", "&lt;");
                return escaped;
            }
        };
    }
}
