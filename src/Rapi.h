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
// - R_ext/RStartup.h

#pragma once
#include "stdafx.h"

extern "C" {
    // Renamed to R_FALSE and R_TRUE to avoid conflicts with Win32 TRUE and FALSE.
    typedef enum { R_FALSE = 0, R_TRUE } Rboolean;

    typedef struct {
        jmp_buf buf;
        int sigmask;
        int savedmask;
    } sigjmp_buf[1];

    typedef struct SEXPREC *SEXP;

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

    typedef void(*R_CFinalizer_t)(SEXP);

    typedef int (*blah1) (const char *, char *, int, int);
    typedef void (*blah2) (const char *, int);
    typedef void (*blah3) (void);
    typedef void (*blah4) (const char *);
    typedef int (*blah5) (const char *);
    typedef void (*blah6) (int);
    typedef void (*blah7) (const char *, int, int);
    typedef enum {RGui, RTerm, LinkDLL} UImode;

    typedef enum {
        SA_NORESTORE,
        SA_RESTORE,
        SA_DEFAULT,
        SA_NOSAVE,
        SA_SAVE,
        SA_SAVEASK,
        SA_SUICIDE
    } SA_TYPE;

    typedef struct
    {
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
    } structRstart;

    typedef structRstart *Rstart;

#ifdef _WIN32
    __declspec(dllimport) extern RCNTXT* R_GlobalContext;
    __declspec(dllimport) extern int R_DirtyImage;
    __declspec(dllimport) extern char *R_TempDir;    
    __declspec(dllimport) extern int UserBreak;
#endif

    extern void R_DefParams(Rstart);
    extern void R_SetParams(Rstart);
    extern void R_SetWin32(Rstart);
    extern void R_SizeFromEnv(Rstart);
    extern void R_common_command_line(int *, char **, Rstart);
    extern void R_set_command_line_arguments(int argc, char **argv);
    extern void setup_Rmainloop(void);
    extern void run_Rmainloop(void);
    extern SEXP Rf_protect(SEXP);
    extern void Rf_unprotect(int);
    extern void Rf_unprotect_ptr(SEXP);
    extern SEXP Rf_mkChar(const char*);
    extern void R_RegisterCFinalizerEx(SEXP s, R_CFinalizer_t fun, Rboolean onexit);
    extern int Rf_initEmbeddedR(int argc, char *argv[]);
    extern void Rf_endEmbeddedR(int fatal);
    extern int Rf_initialize_R(int ac, char **av);
    extern void setup_Rmainloop(void);
    extern void R_ReplDLLinit(void);
    extern int R_ReplDLLdo1(void);
    extern void R_setStartTime(void);
    extern void R_RunExitFinalizers(void);
    extern void CleanEd(void);
    extern void Rf_KillAllDevices(void);
    extern void R_CleanTempDir(void);
    extern void R_SaveGlobalEnv(void);

#ifdef _WIN32
    extern char *getDLLVersion(void), *getRUser(void), *get_R_HOME(void);
    extern void setup_term_ui(void);
    extern Rboolean AllDevicesKilled;
    extern void editorcleanall(void);
    extern int GA_initapp(int, char **);
    extern void GA_appcleanup(void);
    extern void readconsolecfg(void);
#endif
}
