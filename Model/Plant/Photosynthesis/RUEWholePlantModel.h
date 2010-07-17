#ifndef RUEWholePlantModelH
#define RUEWholePlantModelH
#include <stdio.h>
#include <math.h>
#include <map>
#include <string>
#include <stdexcept>
#include <iomanip>

#include <ComponentInterface/DataTypes.h>
#include <General/string_functions.h>
#include <ComponentInterface/ScienceAPI.h>
#include "../Utility/InterpolationFunction.h"

#include "../PlantInterface.h"
#include "../Utility/PlantUtility.h"
#include "PhotosynthesisModel.h"

class RUEWholePlantModel : public PhotosynthesisModel {

  public:
  RUEWholePlantModel(ScienceAPI& scienceAPI, plantInterface& p);
  float PotentialDM (float radiationInterceptedGreen);
  void  Read (void);
  float getRUE (void);

  private:
  float cRUEWholePlant;
};

#endif

