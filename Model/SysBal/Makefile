include $(APSIM)/Model/Build/Platform.make

PROJECT = SysBal

SRC = Registrations.f90 SysBal.for 
ifeq ($(PLATFORM),Linux)
SRC := $(SRC) ../FortranInfrastructure/EntryPointsLinux.for 
DEF = ../Build/gfcomp.ldscript
else
SRC := $(SRC) ../FortranInfrastructure/EntryPoints.for 
DEF = ../Build/gfcomp.def
endif

STATICLIBS = FortranInfrastructure
LIBS = FortranComponentInterface

APSIMMODEL = Yes
PROJECTTYPE = dll

include $(APSIM)/Model/Build/$(PLATFORM)FOR.make

