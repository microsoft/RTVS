// This file combines various bits and pieces of R API in a single header file.
//
// The main reason for its existence is that R headers themselves are not immediately usable
// after checkout: some parts expect R to be at least partially built to generate config.h
// etc, or rely on various macros defined in R makefiles. By moving all this code here and
// cleaning it up, it is readily usable.
//
// The following R header files were used as a basis to produce this file:
// - Rembedded.h
// - Rinternals.h
// - R_ext/Boolean.h
// - R_ext/Rdynload.h
// - R_ext/RStartup.h

#pragma once
#include "stdafx.h"

extern "C" {
    typedef int R_len_t;
    typedef ptrdiff_t R_xlen_t;

    // Renamed to R_FALSE and R_TRUE to avoid conflicts with Win32 FALSE and TRUE.
    typedef enum { R_FALSE = 0, R_TRUE } Rboolean;

    typedef struct {
        jmp_buf buf;
        int sigmask;
        int savedmask;
    } sigjmp_buf[1];

    typedef struct SEXPREC *SEXP;

    enum SEXPTYPE : unsigned int {
        NILSXP = 0,
        SYMSXP = 1,
        LISTSXP = 2,
        CLOSXP = 3,
        ENVSXP = 4,
        PROMSXP = 5,
        LANGSXP = 6,
        SPECIALSXP = 7,
        BUILTINSXP = 8,
        CHARSXP = 9,
        LGLSXP = 10,
        INTSXP = 13,
        REALSXP = 14,
        CPLXSXP = 15,
        STRSXP = 16,
        DOTSXP = 17,
        ANYSXP = 18,
        VECSXP = 19,
        EXPRSXP = 20,
        BCODESXP = 21,
        EXTPTRSXP = 22,
        WEAKREFSXP = 23,
        RAWSXP = 24,
        S4SXP = 25,
        NEWSXP = 30,
        FREESXP = 31,
        FUNSXP = 99,
    };

    typedef struct RCNTXT {
        struct RCNTXT *nextcontext;
        int callflag;
        sigjmp_buf cjmpbuf;
        int cstacktop;
        int evaldepth;
        SEXP promargs;
        SEXP callfun;
        SEXP sysparent;
        SEXP call;
        SEXP cloenv;
        SEXP conexit;
        void(*cend)(void *);
        void *cenddata;
        void *vmax;
        int intsusp;
        SEXP handlerstack;
        SEXP restartstack;
        struct RPRSTACK *prstack;
        SEXP *nodestack;
#ifdef BC_INT_STACK
        IStackval *intstack;
#endif
        SEXP srcref;
    } RCNTXT, *context;

    typedef enum {
        PARSE_NULL,
        PARSE_OK,
        PARSE_INCOMPLETE,
        PARSE_ERROR,
        PARSE_EOF
    } ParseStatus;

    typedef enum {
        RGui,
        RTerm,
        LinkDLL
    } UImode;

    typedef enum {
        SA_NORESTORE,
        SA_RESTORE,
        SA_DEFAULT,
        SA_NOSAVE,
        SA_SAVE,
        SA_SAVEASK,
        SA_SUICIDE
    } SA_TYPE;

    typedef void(*R_CFinalizer_t)(SEXP);
    typedef int(*blah1) (const char *, char *, int, int);
    typedef void(*blah2) (const char *, int);
    typedef void(*blah3) (void);
    typedef void(*blah4) (const char *);
    typedef int(*blah5) (const char *);
    typedef void(*blah6) (int);
    typedef void(*blah7) (const char *, int, int);

    typedef struct {
        Rboolean R_Quiet;
        Rboolean R_Slave;
        Rboolean R_Interactive;
        Rboolean R_Verbose;
        Rboolean LoadSiteFile;
        Rboolean LoadInitFile;
        Rboolean DebugInitFile;
        SA_TYPE RestoreAction;
        SA_TYPE SaveAction;
        size_t vsize;
        size_t nsize;
        size_t max_vsize;
        size_t max_nsize;
        size_t ppsize;
        int NoRenviron;

#ifdef _WIN32
        char *rhome;
        char *home;
        blah1 ReadConsole;
        blah2 WriteConsole;
        blah3 CallBack;
        blah4 ShowMessage;
        blah5 YesNoCancel;
        blah6 Busy;
        UImode CharacterMode;
        blah7 WriteConsoleEx;
#endif
    } structRstart, *Rstart;

#ifdef _WIN32
    __declspec(dllimport) extern UImode CharacterMode;
    __declspec(dllimport) extern RCNTXT* R_GlobalContext;
    __declspec(dllimport) extern SEXP R_GlobalEnv, R_EmptyEnv, R_BaseEnv, R_BaseNamespace, R_Srcref, R_NilValue, R_UnboundValue, R_MissingArg;
    __declspec(dllimport) extern int R_DirtyImage;
    __declspec(dllimport) extern char *R_TempDir;
    __declspec(dllimport) extern int UserBreak;
#endif

    extern void R_DefParams(Rstart);
    extern void R_SetParams(Rstart);
    extern void R_SetWin32(Rstart);
    extern void R_SizeFromEnv(Rstart);
    extern void R_common_command_line(int*, char**, Rstart);
    extern void R_set_command_line_arguments(int argc, char** argv);
    extern void R_RegisterCFinalizerEx(SEXP s, R_CFinalizer_t fun, Rboolean onexit);
    extern void R_ReplDLLinit(void);
    extern int R_ReplDLLdo1(void);
    extern void R_setStartTime(void);
    extern void R_RunExitFinalizers(void);
    extern void Rf_KillAllDevices(void);
    extern void R_CleanTempDir(void);
    extern void R_SaveGlobalEnv(void);
    extern SEXP R_ParseVector(SEXP, int, ParseStatus*, SEXP);
    extern SEXP R_tryEval(SEXP, SEXP, int*);
    extern SEXP R_tryEvalSilent(SEXP, SEXP, int*);
    extern const char *R_curErrorBuf();

    extern int Rf_initialize_R(int ac, char** av);
    extern int Rf_initEmbeddedR(int argc, char** argv);
    extern void Rf_endEmbeddedR(int fatal);
    extern SEXP Rf_protect(SEXP);
    extern void Rf_unprotect(int);
    extern void Rf_unprotect_ptr(SEXP);
    extern SEXP Rf_mkChar(const char*);
    extern SEXP Rf_asChar(SEXP);
    extern SEXP Rf_allocVector3(SEXPTYPE, R_xlen_t, /*R_allocator_t*/ void*);
    extern R_len_t Rf_length(SEXP);

    extern void setup_Rmainloop(void);
    extern void run_Rmainloop(void);
    extern void CleanEd(void);

    extern SEXP STRING_ELT(SEXP x, R_xlen_t i);
    extern SEXP VECTOR_ELT(SEXP x, R_xlen_t i);
    extern void SET_STRING_ELT(SEXP x, R_xlen_t i, SEXP v);
    extern SEXP SET_VECTOR_ELT(SEXP x, R_xlen_t i, SEXP v);
    extern const char* R_CHAR(SEXP x);

#ifdef _WIN32
    extern char *getDLLVersion(void), *getRUser(void), *get_R_HOME(void);
    extern void setup_term_ui(void);
    extern Rboolean AllDevicesKilled;
    extern void editorcleanall(void);
    extern int GA_initapp(int, char**);
    extern void GA_appcleanup(void);
    extern void readconsolecfg(void);
#endif

    typedef void * (*DL_FUNC)();
    typedef unsigned int R_NativePrimitiveArgType;
    typedef unsigned int R_NativeObjectArgType;
    typedef enum { R_ARG_IN, R_ARG_OUT, R_ARG_IN_OUT, R_IRRELEVANT } R_NativeArgStyle;

    typedef struct {
        const char *name;
        DL_FUNC     fun;
        int         numArgs;
        R_NativePrimitiveArgType *types;
        R_NativeArgStyle         *styles;
    } R_CMethodDef;
    typedef R_CMethodDef R_FortranMethodDef;

    typedef struct {
        const char *name;
        DL_FUNC     fun;
        int         numArgs;
    } R_CallMethodDef;
    typedef R_CallMethodDef R_ExternalMethodDef;

    typedef struct _DllInfo DllInfo;

    int R_registerRoutines(DllInfo *info, const R_CMethodDef * const croutines,
        const R_CallMethodDef * const callRoutines,
        const R_FortranMethodDef * const fortranRoutines,
        const R_ExternalMethodDef * const externalRoutines);

    Rboolean R_useDynamicSymbols(DllInfo *info, Rboolean value);
    Rboolean R_forceSymbols(DllInfo *info, Rboolean value);

    DllInfo *R_getEmbeddingDllInfo(void);

    void R_WaitEvent();
    void R_ProcessEvents();
    void R_Suicide(const char *);
}
