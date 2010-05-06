//---------------------------------------------------------------------------

#ifndef ApsimCommandsH
#define ApsimCommandsH
//---------------------------------------------------------------------------
// Send all files to EXCEL.  Files is a CSV list of filenames.
//---------------------------------------------------------------------------
extern "C" _export void __stdcall excelFiles(const char* csvFiles);

extern "C" _export void __stdcall apsimuigraph(const char* csvFiles);
extern "C" _export void __stdcall runapsimgraph(const char* csvFiles);

//---------------------------------------------------------------------------
// Send all files to Apsim.
//---------------------------------------------------------------------------
extern "C" _export void __stdcall runFiles(const char* csvFiles);

//---------------------------------------------------------------------------
// Convert all files to SIM format.
//---------------------------------------------------------------------------
extern "C" _export void __stdcall createSimFiles(const char* csvFiles);

//---------------------------------------------------------------------------
// Send all files to an editor.
//---------------------------------------------------------------------------
extern "C" _export void __stdcall viewFiles(const char* csvFiles);

//---------------------------------------------------------------------------
// Open an .apsim file.
//---------------------------------------------------------------------------
extern "C" _export void __stdcall apsimFiles(const char* csvFiles);

//---------------------------------------------------------------------------
// Probe a types file or plugin file
//---------------------------------------------------------------------------
extern "C" _export void __stdcall probeFile(const char* csvFiles);

#endif
