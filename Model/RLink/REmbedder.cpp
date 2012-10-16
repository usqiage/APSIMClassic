//---------------------------------------------------------------------------
// The windows build of this file is built with gcc. It has no apsim infrastructure calls. We cannot
// pass c++ classes as arguments - the name mangling and memory managers are incompatible.
// It requires RTools (from CRAN) to compile.

#include <unistd.h>
#include <RInside.h>
#include "REmbedder.h"
#include "RDataTypes.h"

V_CHAR4_FN apsimCallback = NULL;

SEXP apsimGet( std::string variableName )
{
   SEXP result = Rcpp::wrap(NA_STRING);
   if (apsimCallback == NULL) return result;

   char typeReturned[20];
   char dataReturned[8192]; 
   apsimCallback("get", variableName.c_str(), typeReturned, (void *)dataReturned);
   if (!strcmp(typeReturned, "numeric")) {
      result = Rcpp::wrap(* (double*)dataReturned);
   } else if (!strcmp(typeReturned, "string")) {
      result = Rcpp::wrap(std::string((char *) dataReturned));
   } else if (!strcmp(typeReturned, "numericvec")) {
      int numReturned = * (int *) dataReturned; 
      std::vector<double> v;
      double *buf = (double *) ((char *) dataReturned + sizeof(int));
      for (int i = 0; i < numReturned; i++) { v.push_back(buf[i]); }
      result = Rcpp::wrap(v);
   } else if (!strcmp(typeReturned, "stringvec")) {
      std::vector<std::string> v;
      int numReturned = (int) *dataReturned;
      char *buf = ((char *) dataReturned + sizeof(int));
      for (int i = 0; i < numReturned; i++) { v.push_back(buf); buf += strlen(buf) + 1; }
      result = Rcpp::wrap(v);
   }
   return result;
}

void apsimSet( std::string variableName, SEXP value )
{
   if (apsimCallback == NULL) return;
   Rcpp::Function f = Rcpp::Function("class");
   std::string className = Rcpp::as<std::string>( f(value));
   Rcpp::Function g = Rcpp::Function("length");
   int argLength = Rcpp::as<int>( g(value));
   if (argLength == 0) 
     apsimCallback("fatal", ("Cant set nil value to " + variableName).c_str(), NULL, NULL);

   if (className == "character")
     if (argLength > 1) 
       {
       std::vector<std::string> v = Rcpp::as< std::vector<std::string> >(value);
       int numbytes = sizeof(int);
       for (unsigned int i = 0; i < v.size(); i++) {numbytes += v[i].length() + 1; }
       char *buf = new char [numbytes] ;
       *((int*)buf) = v.size();
       char *ptr = buf + sizeof(int);
       for (unsigned int i = 0; i < v.size(); i++) { strcpy(ptr, v[i].c_str()); ptr += strlen(ptr) + 1; }
       apsimCallback("set", variableName.c_str(), "charvec", (void *)buf); 
       delete [] buf;
       }
     else
       {
       std::string v = Rcpp::as< std::string >(value);
       char *buf = new char [v.length()+1] ;
       strcpy(buf, v.c_str());
       apsimCallback("set", variableName.c_str(), "char", (void *)buf); 
       delete [] buf;
       }
   else if (className == "numeric")
     if (argLength > 1) 
       {
       std::vector<double> v = Rcpp::as< std::vector<double> >(value);
       int numbytes = sizeof(int) + v.size() * sizeof(double);
       char *buf = new char [numbytes] ;
       *((int*)buf) = v.size();
       double *ptr = (double *) (buf + sizeof(int));
       for (unsigned int i = 0; i < v.size(); i++) { ptr[i] = v[i]; }
       apsimCallback("set", variableName.c_str(), "numericvec", (void *)buf); 
       delete [] buf;
       }
     else
       {
       double v = Rcpp::as< double >(value);
       apsimCallback("set", variableName.c_str(), "numeric", (void *)&v); 
       }
   else
     apsimCallback("fatal", ("Unknown class " + className).c_str(), NULL, NULL);

   return;
}

void apsimSubscribe( std::string eventName, std::string typeName, std::string handler) 
   {
   char buf[1024];
   strcpy(buf, typeName.c_str());
   if (apsimCallback != NULL) 
     apsimCallback("subscribe", eventName.c_str(), buf, (void *)handler.c_str());
   }

void apsimExpose( std::string variableName, std::string units) 
   {
   char buf[1024];
   strcpy(buf, units.c_str());
   if (apsimCallback != NULL) 
     apsimCallback("expose", variableName.c_str(), buf, NULL);
   }

// apsimPublish() has autogenerated parts. Include it
#include "apsimPublish.cpp"

void apsimFatalError( std::string message ) 
  {
  if (apsimCallback != NULL) 
    apsimCallback("fatal", message.c_str(), NULL, NULL);
  }

SEXP apsimQuery( std::string pattern ) 
  {
   SEXP result = Rcpp::wrap(R_NilValue);
   if (apsimCallback == NULL) return result;

   char typeReturned[20];
   char dataReturned[8192]; 
   apsimCallback("query", pattern.c_str(), typeReturned, (void *)dataReturned);
   std::vector<std::string> v;
   int numReturned = (int) *dataReturned;
   char *buf = ((char *) dataReturned + sizeof(int));
   for (int i = 0; i < numReturned; i++) { v.push_back(buf); buf += strlen(buf) + 1; }
   result = Rcpp::wrap(v);
   return(result);
}
  
RCPP_MODULE(apsim){
  using namespace Rcpp ;
  function( "publish",  &apsimPublish, List::create( _["eventName"], _["type"], _["value"]),     "Publish an apsim event" ) ;
  function( "subscribe",&apsimSubscribe, List::create( _["eventName"], _["type"], _["handler"]), "Subscribe to an apsim event" ) ;
  function( "get",      &apsimGet, List::create( _["variableName"]),                       "Get an apsim variable" ) ;
  function( "set",      &apsimSet, List::create( _["variableName"], _["variableValue"] ),  "Set an apsim variable" ) ;
  function( "expose",   &apsimExpose, List::create( _["variableName"] , _["units"] = "" ), "Expose an R variable to apsim" ) ;
  function( "query",    &apsimQuery, List::create( _["pattern"] ),                         "Query apsim for a module or variable" ) ;
  function( "fatal",    &apsimFatalError, List::create( _["message"] ),                    "Signal a fatal apsim error" ) ;
}  

///////////////////START
RInside *R = NULL;
// These "Embedded_xxxx" routines are called via dlsym() from the apsim component
// Return false on error
extern "C" bool EmbeddedR_Start(const char *R_Home, const char *UserLibs)
{
   int argc = 0;
   char **argv = NULL;
#ifdef WIN32
   std::string newPath = getenv("PATH");
   std::replace(newPath.begin(), newPath.end(), '\\', '/'); 
   newPath = std::string(R_Home) + "/bin/i386;" + newPath;
   setenv("PATH", newPath.c_str(), 1);

   char oldWD [8192];
   getcwd(oldWD, 8192);
   std::string newWD = std::string(R_Home) + "/bin/i386";
   chdir(newWD.c_str());
#endif

   if (R_Home != NULL && strlen(R_Home) > 0) { setenv("R_HOME", R_Home, 1) ; }
   if (UserLibs != NULL && strlen(UserLibs) > 0) { setenv("R_LIBS_USER", UserLibs, 1); }
   // TMPDIR??
   // R_USER??

   try {
     R = new RInside(argc, argv, true); 
   } catch (std::exception& ex) {
        std::cerr << "R Exception: " << ex.what() << std::endl; std::cerr.flush();
		return false; 
   } 	 

   try {
     R->parseEval("library(\"methods\")");
     (*R)["apsim"] = LOAD_RCPP_MODULE(apsim) ;
   } catch (std::exception& ex) {
        std::cerr << "R Exception: " << ex.what() << std::endl; std::cerr.flush();
        std::string msg = R->parseEval("geterrmessage()");
        std::cerr << msg << std::endl; std::cerr.flush();
		return false; 
   }

   R->parseEval("apsimPublish<-apsim$publish");
   R->parseEval("apsimSubscribe<-apsim$subscribe");
   R->parseEval("apsimGet<-apsim$get");
   R->parseEval("apsimSet<-apsim$set");
   R->parseEval("apsimExpose<-apsim$expose");
   R->parseEval("apsimFatal<-apsim$fatal");

#ifdef WIN32
   chdir(oldWD);
#endif
   
   return true;
}

extern "C" bool EmbeddedR_Stop(void)
{
  if (R == NULL)
     return false;

  delete R;

  R = NULL;
  return true;
}

extern "C" bool EmbeddedR_SetComponent(void *fPtr)
{
  apsimCallback = (V_CHAR4_FN) fPtr;
  return true;
}

extern "C" bool EmbeddedR_Eval(const char *cmd)
{
  if (R == NULL)
     return false;

  R->parseEval(cmd);
  return true;
}

extern "C" bool EmbeddedR_GetVector(const char *variable, char *result, 
                                    unsigned int resultlen, unsigned int elemwidth, 
                                    unsigned int *numReturned)
{
  *numReturned = 0;
  if (R == NULL)
     return false;
  try 
    {
    Rcpp::CharacterVector resultVec = (*R)[variable];
    char *ptr = result;
    for (int i = 0; i < resultVec.size() && ptr  < result + resultlen; i++)
        {
        const char *stringrep = resultVec[i];
        strncpy(ptr, stringrep, elemwidth);
        ptr[elemwidth-1] = '\0';
        ptr += elemwidth;
        (*numReturned)++;
        }		
    return true;
    } 
  catch(std::exception& ex) {
    std::cout << "R Exception getting " << variable << ":" << ex.what() << std::endl;
    std::string msg = R->parseEval("geterrmessage()");
    std::cout << msg << std::endl; std::cout.flush();
  }
  return false;
}

extern "C" bool EmbeddedR_SimpleCharEval(const char *cmd, char *buf, int buflen)
{
  buf[0] = '\0';
  if (R == NULL)
     return false;

  std::string result = R->parseEval(cmd);
  strncpy(buf, result.c_str(), buflen);
  return true;
}

#ifndef WIN32
// for gcc builds under unix
void StopR(void) {EmbeddedR_Stop();}

void REvalQ(const char *s) 
  {
  if (R != NULL)
     R->parseEval(s);
  }

void RGetVector(const char *variable, std::vector<std::string> &result)
  {
  result.clear();
  if (R != NULL) 
     {
     Rcpp::CharacterVector resultVec = (*R)[variable];
     for (int i = 0; i < resultVec.size(); i++)
       result.push_back(std::string(resultVec[i]));
     }
  }

std::string SimpleREval(const char *s)
  {
  if (R != NULL)
     {
     std::string result = R->parseEval(s);
     return result;
     }
  return "";  
  }

#endif
