# Makefile for R component

# There are 2 dlls produced - one loaded by apsim; the other once the location of
# the R installation is known.

# On windows, the first (RLink.dll) is built by msvc, the other (REmbed.dll) by gcc.
 
include $(APSIM)/Model/Build/Platform.make

PROJECT = RLink
PROJECTTYPE = dll
ifeq ($(PLATFORM),Linux)
  OBJEXT = o
else
  OBJEXT = obj
endif

LIBS = General ApsimShared ComponentInterface2
OBJS = $(APSIM)/Model/ComponentInterface2/CMPComponentEntryPoints.$(OBJEXT)
SRC = RComponent.cpp RDataTypes_1.cpp RDataTypes_2.cpp

include $(APSIM)/Model/Build/$(PLATFORM)CPP.make

RComponent.$(OBJEXT): RComponent.cpp RComponent.h RDataTypes.h

RDataTypes_1.$(OBJEXT): RDataTypes_1.cpp RDataTypes.h

RDataTypes_2.$(OBJEXT): RDataTypes_2.cpp RDataTypes.h

RComponent.h RDataTypes_1.cpp RDataTypes_2.cpp  RDataTypes.h REmbed.cpp REmbed.h RLink.def: ../DataTypes/Datatypes.xml RDataTypes.macro
	$(APSIM)/Model/ProcessDataTypesInterface.exe ../DataTypes/Datatypes.xml RDataTypes.macro

libRLink.a: RLink.def
	dlltool -d RLink.def -l libRLink.a

clean:
	rm -f RComponent.h RDataTypes*.* RDataTypesEmbedded.cpp RDataTypesEmbedded.h RLink.def ../REmbed.so ../RSimulation.so *.o 