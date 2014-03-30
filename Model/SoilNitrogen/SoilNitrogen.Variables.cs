﻿using System;
using System.Reflection;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml;
using System.Linq;
using System.Text;
using ModelFramework;
using CSGeneral;

/// <remarks>
/// This partial class contains most of the variables and input properties of SoilNitrogen
/// </remarks>
public partial class SoilNitrogen
{
    #region Links to other modules

    /// <summary>
    /// Link to APSIM's clock (time information)
    /// </summary>
    [Link]
    public Clock Clock = null;

    /// <summary>
    /// Link to APSIM's metFile (weather data)
    /// </summary>
    [Link]
    public MetFile MetFile = null;

    /// <summary>
    /// Link to container paddock
    /// </summary>
    [Link]
    public Paddock Paddock;		// not sure why it is here

    #endregion

    #region Parameters and inputs provided by the user or APSIM

    #region Parameters used on initialisation only

    #region General setting parameters

    /// <summary>
    /// Soil parameterisation set to use
    /// </summary>
    /// <remarks>
    /// Used to determine which node of xml file will be used to overwrite some [Param]'s
    /// </remarks>
    [Param(IsOptional = true)]
    [Description("Soil parameterisation set to use")]
    public string SoilNParameterSet= "standard";

    /// <summary>
    /// Indicates whether simpleSoilTemp is allowed
    /// </summary>
    /// <remarks>
    /// When 'yes', soil temperature may be computed internally, if an external value is not supplied.
    /// If 'no', a value for soil temperature must be supplied or an fatal error will occur.
    /// </remarks>
    private bool SimpleSoilTempAllowed = false;
    [Param]
    [Description("Indicates whether simpleSoilTemp is allowed")]
    public string allowSimpleSoilTemp
    {
        get { return (SimpleSoilTempAllowed) ? "yes" : "no"; }
        set { SimpleSoilTempAllowed = value.ToLower().Contains("yes"); }
    }

    /// <summary>
    /// Indicates whether soil profile reduction is allowed (from erosion)
    /// </summary> 
    private bool ProfileReductionAllowed = false;
    [Param]
    [Description("Indicates whether soil profile reduction is allowed")]
    public string allowProfileReduction
    {
        get { return (ProfileReductionAllowed) ? "yes" : "no"; }
        set { ProfileReductionAllowed = value.ToLower().StartsWith("on"); }
    }

    /// <summary>
    /// Indicates whether organic solutes are to be simulated
    /// </summary>
    /// <remarks>
    /// It should always be false, as organic solutes are not implemented yet
    /// </remarks>
    private bool OrganicSolutesAllowed = false;
    [Param(IsOptional = true)]
    [Description("Indicates whether organic solutes are to be simulated")]
    public string allowOrganicSolutes
    {
        get { return (OrganicSolutesAllowed) ? "yes" : "no"; }
        set { OrganicSolutesAllowed = value.ToLower().StartsWith("on"); }
    }

    /// <summary>
    /// Factor to convert from OC to OM
    /// </summary>
    [Param(MinVal = 0.0, MaxVal = 3.0)]
    [Units("")]
    [Description("Factor to convert from OC to OM")]
    public double defaultCarbonInSoilOM;

    /// <summary>
    /// Default weight fraction of C in carbohydrates
    /// </summary>
    /// <remarks>
    /// Used to convert FOM amount into fom_c
    /// </remarks>
    [Param(IsOptional = true, MinVal = 0.0, MaxVal = 1.0)]
    [Units("0-1")]
    [Description("Weight fraction of C in FOM")]
    public double defaultCarbonInFOM = 0.4;

    /// <summary>
    /// Default value for initialising soil pH
    /// </summary>
    [Param()]
    [Units("")]
    [Description("Default value for pH at initialisation")]
    public double defaultInitialpH;

    /// <summary>
    /// Threshold value to trigger a warning message when negative values are detected
    /// </summary>
    [Param()]
    [Units("")]
    [Description("Threshold value to trigger a warning message when negative values are detected")]
    public double WarningNegativeThreshold;

    /// <summary>
    /// Threshold value to trigger a fatal error when negative values are detected
    /// </summary>
    [Param()]
    [Units("")]
    [Description("Threshold value to trigger a fatal error when negative values are detected")]
    public double FatalNegativeThreshold;

    #endregion general settings

    #region Parameters for setting up soil organic matter

    /// <summary>
    /// The C:N ratio of the soil humus (active + inert)
    /// </summary>
    /// <remarks>
    /// Remains fixed throughout the simulation
    /// </remarks>
    [Param(MinVal = 1.0, MaxVal = 25.0)]
    [Units("")]
    [Description("The C:N ratio of the soil OM (humus)")]
    public double HumusCNr = 0.0;

    /// <summary>
    /// The C:N ratio of microbial biomass
    /// </summary>
    /// <remarks>
    /// Remains fixed throughout the simulation
    /// </remarks>
    [Param(IsOptional = true, MinVal = 1.0, MaxVal = 50.0)]
    [Units("")]
    [Description("The C:N ratio of microbial biomass")]
    public double MBiomassCNr = 8.0;

    /// <summary>
    /// Proportion of biomass-C in the initial mineralizable humic-C (0-1)
    /// </summary>
    [Param(MinVal = 0.0, MaxVal = 1.0)]
    [Units("0-1")]
    [Description("Fraction of biomass in the active humus")]
    public double[] fbiom;

    /// <summary>
    /// Proportion of the initial total soil C that is inert, not subject to mineralisation (0-1)
    /// </summary>
    [Param(MinVal = 0.0, MaxVal = 1.0)]
    [Units("0-1")]
    [Description("Fraction humus that is inert")]
    public double[] finert;

    #endregion params for OM setup

    #region Parameters for setting up fresh organic matter (FOM)

    /// <summary>
    /// Initial amount of FOM in the soil (kgDM/ha)
    /// </summary>
    [Param(IsOptional = true, MinVal = 0.0, MaxVal = 100000.0)]
    [Units("kg/ha")]
    [Description("Initial amount of FOM in the soil")]
    public double InitialFOMAmount = 0.0;

    /// <summary>
    /// Initial depth over which FOM is distributed within the soil profile (mm)
    /// </summary>
    /// <remarks>
    /// If not given fom will be distributed over the whole soil profile
    /// Distribution follows an exponential function
    /// </remarks>
    [Param(IsOptional = true)]
    [Units("mm")]
    [Description("Initial depth over which FOM is distributed in the soil")]
    public double InitialFOMDepth = -99.0;

    /// <summary>
    /// Exponent of function used to compute initial distribution of FOM in the soil
    /// </summary>
    /// <remarks>
    /// If not given, a default value  might be considered (3.0)
    /// </remarks>
    [Param(IsOptional = true, MinVal = 0.01, MaxVal = 10.0)]
    [Units("")]
    [Description("Exponent for the FOM distribution in soil")]
    public double FOMDistributionCoefficient = 3.0;

    /// <summary>
    /// Initial C:N ratio of roots (actually FOM)
    /// </summary>
    /// <remarks>
    /// This may not be used if values for each pool are given, thus it is optional
    /// however, a default value is always needed as it may happen that neither is given
    /// </remarks>
    [Param(IsOptional = true, MinVal = 0.1, MaxVal = 750.0)]
    [Units("")]
    [Description("Initial C:N ratio of FOM")]
    public double InitialCNrFOM = 40.0;

    /// <summary>
    /// FOM type to be used on initialisation
    /// </summary>
    /// <remarks>
    /// This sets the partition of FOM C between the different pools (carbohydrate, cellulose, lignine)
    /// A default value (0) is always assumed
    /// </remarks>
    private int FOMtypeID_reset = 0;
    [Param(IsOptional = true)]
    [Description("FOM type to be used on initialisation")]
    public string InitialFOMType
    {
        get { return fom_types[FOMtypeID_reset]; }
        set
        {
            FOMtypeID_reset = 0;
            for (int i = 0; i < fom_types.Length; i++)
            {
                if (fom_types[i] == value)
                {
                    FOMtypeID_reset = i;
                    break;
                }
            }
            if (InitialFOMType != fom_types[FOMtypeID_reset])
            {   // no valid FOM type was given, use default
                FOMtypeID_reset = 0;
                // let the user know that the default type will be used
                Console.WriteLine("     !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                Console.WriteLine("                    APSIM Warning Error");
                Console.WriteLine("      The initial FOM type was not found, the default type will be used");
                Console.WriteLine("     !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            }
        }
    }

    /// <summary>
    /// List of available FOM types names
    /// </summary>
    [Param(Name = "fom_type")]
    [XmlArray("fom_type")]
    [Description("List of available FOM types names")]
    public String[] fom_types;

    /// <summary>
    /// Fraction of carbohydrate in FOM (0-1), for each FOM type
    /// </summary>
    [Param(MinVal = 0.0, MaxVal = 1.0)]
    [Units("0-1")]
    [Description("Fraction of carbohydrate in FOM, for each FOM type")]
    public double[] fract_carb;

    /// <summary>
    /// Fraction of cellulose in FOM (0-1), for each FOM type
    /// </summary>
    [Param(MinVal = 0.0, MaxVal = 1.0)]
    [Units("0-1")]
    [Description("Fraction of cellulose in FOM, for each FOM type")]
    public double[] fract_cell;

    /// <summary>
    /// Fraction of lignin in FOM (0-1), for each FOM type
    /// </summary>
    [Param(MinVal = 0.0, MaxVal = 1.0)]
    [Units("0-1")]
    [Description("Fraction of lignin in FOM, for each FOM type")]
    public double[] fract_lign;

    #endregion  params for FOM setup

    #region Parameters for the decomposition process of FOM and SurfaceOM

    #region Surface OM

    /// <summary>
    /// Fraction of residue C mineralised retained in the soil OM (0-1)
    /// </summary>
    [Param(MinVal = 0.0, MaxVal = 1.0)]
    [Units("0-1")]
    [Description("Fraction of residue C mineralised retained in the soil OM")]
    public double ResiduesRespirationFactor;

    /// <summary>
    /// Fraction of retained residue C transferred to biomass (0-1)
    /// </summary>
    /// <remarks>
    /// Remaining will got into humus
    /// </remarks>
    [Param(MinVal = 0.0, MaxVal = 1.0)]
    [Units("0-1")]
    [Description("Fraction of retained residue C transferred to biomass")]
    public double ResiduesFractionIntoBiomass;

    /// <summary>
    /// Depth from which mineral N can be immobilised when decomposing surface residues (mm)
    /// </summary>
    [Param(MinVal = 0.0, MaxVal = 1000.0)]
    [Units("mm")]
    [Description("Depth from which mineral N can be immobilised when decomposing surface residues")]
    public double ImmobilisationDepth;

    #endregion

    #region Fresh OM

    /// <summary>
    /// Optimum rate for decomposition of FOM pools [carbohydrate component] (0-1)
    /// </summary>
    [Param(MinVal = 0.0, MaxVal = 1.0)]
    [Units("0-1")]
    [Description("Optimum decomposition rate of FOM carbohydrate")]
    public double[] FOMCarbTurnOverRate;

    /// <summary>
    /// Optimum rate for decomposition of FOM pools [cellulose component] (0-1)
    /// </summary>
    [Param(MinVal = 0.0, MaxVal = 1.0)]
    [Units("0-1")]
    [Description("Optimum decomposition rate of FOM cellulose")]
    public double[] FOMCellTurnOverRate;

    /// <summary>
    /// Optimum rate for decomposition of FOM pools [lignin component] (0-1)
    /// </summary>
    [Param(MinVal = 0.0, MaxVal = 1.0)]
    [Units("0-1")]
    [Description("Optimum decomposition rate of FOM lignine")]
    public double[] FOMLignTurnOverRate;

    /// <summary>
    /// Fraction of the FOM C decomposed retained in the soil OM (0-1)
    /// </summary>
    [Param(MinVal = 0.0, MaxVal = 1.0)]
    [Units("0-1")]
    [Description("Fraction of the FOM C decomposed retained in the soil OM")]
    public double FOMRespirationFactor;

    /// <summary>
    /// Fraction of the retained FOM C transferred to biomass (0-1)
    /// </summary>
    [Param(MinVal = 0.0, MaxVal = 1.0)]
    [Units("0-1")]
    [Description("Fraction of the retained FOM C transferred to biomass")]
    public double FOMFractionIntoBiomass;

    #region Limiting factors

    /// <summary>
    /// Coeff. to determine the magnitude of C:N effects on decomposition of FOM
    /// </summary>
    [Param(MinVal = 0.0, MaxVal = 10.0)]
    public double cnrf_ReductionCoeff;

    /// <summary>
    /// C:N above which decomposition rate of FOM declines
    /// </summary>
    [Param(MinVal = 5.0, MaxVal = 100.0)]
    public double cnrf_CNthreshold;

    /// <summary>
    /// Data for calculating the temperature effect on FOM decomposition
    /// </summary>
    private BentStickData TempFactorData_DecompFOM = new BentStickData();

    /// <summary>
    /// Optimum temperature for decomposition of FOM
    /// </summary>
    [Param]
    [Units("oC")]
    [Description("Optimum temperature for decomposition of FOM")]
    public double[] stf_DecompFOM_Topt
    {
        get { return TempFactorData_DecompFOM.xValueForOptimum; }
        set { TempFactorData_DecompFOM.xValueForOptimum = value; }
    }

    /// <summary>
    /// Temperature factor for decomposition of FOM at zero degrees
    /// </summary>
    [Param]
    [Units("0-1")]
    [Description("Temperature factor for decomposition of FOM at zero degrees")]
    public double[] stf_DecompFOM_FctrZero
    {
        get { return TempFactorData_DecompFOM.yValueAtZero; }
        set { TempFactorData_DecompFOM.yValueAtZero = value; }
    }

    /// <summary>
    /// Curve exponent for temperature factor for decomposition of FOM
    /// </summary>
    [Param]
    [Units("")]
    [Description("Curve exponent for temperature factor")]
    public double[] stf_DecompFOM_CvExp
    {
        get { return TempFactorData_DecompFOM.CurveExponent; }
        set { TempFactorData_DecompFOM.CurveExponent = value; }
    }

    /// <summary>
    /// Parameters for calculating the soil moisture factor for FOM decomposition
    /// </summary>
    private BrokenStickData MoistFactorData_DecompFOM = new BrokenStickData();

    /// <summary>
    /// Values of modified soil water content at which the moisture factor is given
    /// </summary>
    [Param]
    [Units("0-3")]
    [Description("X values for the moisture factor function")]
    public double[] swf_DecompFOM_swx
    { set { MoistFactorData_DecompFOM.xVals = value; } }

    /// <summary>
    /// Moiture factor values for the given modified soil water content
    /// </summary>
    [Param]
    [Units("0-1")]
    [Description("Y values for the moisture factor function")]
    public double[] swf_DecompFOM_y
    { set { MoistFactorData_DecompFOM.yVals = value; } }

    #endregion

    #endregion FOM

    #endregion params for SurfOM + FOM decompostion

    #region Parameters for SOM mineralisation/immobilisation process

    /// <summary>
    /// Potential rate of soil biomass mineralisation (fraction per day)
    /// </summary>
    [Param(MinVal = 0.0, MaxVal = 1.0)]
    [Units("0-1")]
    [Description("Potential rate of soil biomass mineralisation")]
    public double[] MBiomassTurnOverRate;

    /// <summary>
    /// Fraction of biomass C mineralised retained in soil OM (0-1)
    /// </summary>
    [Param(MinVal = 0.0, MaxVal = 1.0)]
    [Units("0-1")]
    [Description("Fraction of biomass C mineralised retained in soil OM")]
    public double MBiomassRespirationFactor;

    /// <summary>
    /// Fraction of retained biomass C returned to biomass (0-1)
    /// </summary>
    [Param(MinVal = 0.0, MaxVal = 1.0)]
    [Units("0-1")]
    [Description("Fraction of retained biomass C returned to biomass")]
    public double MBiomassFractionIntoBiomass;

    /// <summary>
    /// Potential rate of humus mineralisation (per day, 0-1)
    /// </summary>
    [Param(MinVal = 0.0, MaxVal = 1.0)]
    [Units("0-1")]
    [Description("Potential rate of humus mineralisation")]
    public double[] AHumusTurnOverRate;

    /// <summary>
    /// Fraction of humic C mineralised retained in soil OM (0-1)
    /// </summary>
    [Param(MinVal = 0.0, MaxVal = 1.0)]
    [Units("0-1")]
    [Description("Fraction of humic C mineralised retained in soil OM")]
    public double AHumusRespirationFactor;

    #region Limiting factors

    /// <summary>
    /// Data to calculate the temperature effect on soil OM mineralisation
    /// </summary>
    private BentStickData TempFactorData_MinerSOM = new BentStickData();

    /// <summary>
    /// Optimum temperature for soil OM mineralisation
    /// </summary>
    [Param]
    [Units("oC")]
    [Description("Optimum temperature for mineralisation of soil OM")]
    public double[] stf_MinerSOM_Topt
    {
        get { return TempFactorData_MinerSOM.xValueForOptimum; }
        set { TempFactorData_MinerSOM.xValueForOptimum = value; }
    }

    /// <summary>
    /// Temperature factor for soil OM mineralisation at zero degree
    /// </summary>
    [Param(MinVal = 0.0, MaxVal = 1.0)]
    [Units("0-1")]
    [Description("Temperature factor for mineralisation of soil OM at zero degrees")]
    public double[] stf_MinerSOM_FctrZero
    {
        get { return TempFactorData_MinerSOM.yValueAtZero; }
        set { TempFactorData_MinerSOM.yValueAtZero = value; }
    }

    /// <summary>
    /// Curve exponent to calculate temperature factor for soil OM mineralisation
    /// </summary>
    [Param]
    [Units("")]
    [Description("Curve exponent for temperature factor")]
    public double[] stf_MinerSOM_CvExp
    {
        get { return TempFactorData_MinerSOM.CurveExponent; }
        set { TempFactorData_MinerSOM.CurveExponent = value; }
    }

    /// <summary>
    /// Parameters to calculate soil moisture factor for soil OM mineralisation
    /// </summary>
    /// <remarks>
    /// These are pairs of points representing a broken stick function
    /// </remarks>
    private BrokenStickData MoistFactorData_MinerSOM = new BrokenStickData();

    /// <summary>
    /// Values of the modified soil water content at which misture factor is know
    /// </summary>
    [Param(MinVal = 0.0, MaxVal = 3.0)]
    [Units("0-3")]
    [Description("X values for the moisture factor function")]
    public double[] swf_MinerSOM_swx
    {
        get { return MoistFactorData_MinerSOM.xVals; }
        set { MoistFactorData_MinerSOM.xVals = value; }
    }

    /// <summary>
    /// Values of the moisture factor at the given modified water content
    /// </summary>
    [Param(MinVal = 0.0, MaxVal = 1.0)]
    [Units("0-1")]
    [Description("Y values for the moisture factor function")]
    public double[] swf_MinerSOM_y
    {
        get { return MoistFactorData_MinerSOM.yVals; }
        set { MoistFactorData_MinerSOM.yVals = value; }
    }

    #endregion

    #endregion params for OM decomposition

    #region Parameters for urea hydrolisys process

    /// <summary>
    /// Parameters to calculate the temperature effect on urea hydrolysis
    /// </summary>
    private BentStickData TempFactorData_UHydrol = new BentStickData();

    /// <summary>
    /// Optimum temperature for urea hydrolisys
    /// </summary>
    [Param(MinVal = 5.0, MaxVal = 100.0)]
    [Units("oC")]
    [Description("Optimum temperature for urea hydrolysis")]
    public double[] stf_Hydrol_Topt
    {
        get { return TempFactorData_UHydrol.xValueForOptimum; }
        set { TempFactorData_UHydrol.xValueForOptimum = value; }
    }

    /// <summary>
    /// Temperature factor for urea hydrolisys at zero degrees
    /// </summary>
    [Param(MinVal = 0.0, MaxVal = 1.0)]
    [Units("0-1")]
    [Description("Temperature factor for urea hydrolisys at zero degrees")]
    public double[] stf_Hydrol_FctrZero
    {
        get { return TempFactorData_UHydrol.yValueAtZero; }
        set { TempFactorData_UHydrol.yValueAtZero = value; }
    }

    /// <summary>
    /// Curve exponent to calculate the temperature factor for urea hydrolisys
    /// </summary>
    [Param]
    [Units("")]
    [Description("Curve exponent for temperature factor")]
    public double[] stf_Hydrol_CvExp
    {
        get { return TempFactorData_UHydrol.CurveExponent; }
        set { TempFactorData_UHydrol.CurveExponent = value; }
    }

    /// <summary>
    /// Parameters to calculate the moisture effect on urea hydrolysis
    /// </summary>
    private BrokenStickData MoistFactorData_UHydrol = new BrokenStickData();

    /// <summary>
    /// Values of the modified soil water content at which factor is known
    /// </summary>
    [Param(MinVal = 0.0, MaxVal = 3.0)]
    [Units("0-3")]
    [Description("X values for the moisture factor function")]
    public double[] swf_Hydrol_swx
    {
        get { return MoistFactorData_UHydrol.xVals; }
        set { MoistFactorData_UHydrol.xVals = value; }
    }

    /// <summary>
    /// Values of the modified moisture factor at given water content
    /// </summary>
    [Param(MinVal = 0.0, MaxVal = 1.0)]
    [Units("0-1")]
    [Description("Y values for the moisture factor function")]
    public double[] swf_Hydrol_y
    {
        get { return MoistFactorData_UHydrol.yVals; }
        set { MoistFactorData_UHydrol.yVals = value; }
    }

    /// Parameters for calculating the potential urea hydrolisys
    /// <summary>
    /// Minimum value for hydrolysis rate
    /// </summary>
    [Param(MinVal = 0.0, MaxVal = 1.0)]
    [Units("0-1")]
    [Description("Minimum value for hydrolysis rate")]
    public double potHydrol_min;

    /// <summary>
    /// Parameter A of the function determining potential urea hydrolysis
    /// </summary>
    [Param]
    [Units("")]
    [Description("Parameter A of the potential urea hydrolysis function")]
    public double potHydrol_parmA;

    /// <summary>
    /// Parameter B of the function determining potential urea hydrolysis
    /// </summary>
    [Param]
    [Units("")]
    [Description("Parameter B of the potential urea hydrolysis function")]
    public double potHydrol_parmB;

    /// <summary>
    /// Parameter C of the function determining potential urea hydrolysis
    /// </summary>
    [Param]
    [Units("")]
    [Description("Parameter C of the potential urea hydrolysis function")]
    public double potHydrol_parmC;

    /// <summary>
    /// Parameter D of the function determining potential urea hydrolysis
    /// </summary>
    [Param]
    [Units("")]
    [Description("Parameter D of the potential urea hydrolysis function")]
    public double potHydrol_parmD;

    #endregion params for hydrolysis

    #region Parameters for nitrification process

    /// <summary>
    /// Maximum potential nitrification (ppm/day)
    /// </summary>
    /// <remarks>
    /// This is the parameter M on Michaelis-Menten equation
    /// r = MC/(k+C)
    /// </remarks>
    [Param(MinVal = 0.0, MaxVal = 100.0)]
    [Units("ppm/day")]
    [Description("Maximum potential nitrification")]
    public double nitrification_pot;

    /// <summary>
    /// NH4 concentration when nitrification rate is half of potential (ppm)
    /// </summary>
    /// <remarks>
    /// This is the parameter k on Michaelis-Menten equation
    /// r = MC/(k+C)
    /// </remarks>
    [Param(MinVal = 0.0, MaxVal = 200.0)]
    [Units("ppm")]
    [Description("NH4 concentration when nitrification rate is half of potential")]
    public double nh4_at_half_pot;

    #region Limiting factors

    /// <summary>
    /// Parameters to calculate the temperature effect on nitrification
    /// </summary>
    private BentStickData TempFactorData_Nitrif = new BentStickData();

    /// <summary>
    /// Optimum temperature for nitrification
    /// </summary>
    [Param(MinVal = 5.0, MaxVal = 100.0)]
    [Units("oC")]
    [Description("Optimum temperature for nitrification")]
    public double[] stf_Nitrif_Topt
    {
        get { return TempFactorData_Nitrif.xValueForOptimum; }
        set { TempFactorData_Nitrif.xValueForOptimum = value; }
    }

    /// <summary>
    /// Temperature factor for nitrification at zero degrees
    /// </summary>
    [Param(MinVal = 0.0, MaxVal = 1.0)]
    [Units("0-1")]
    [Description("Temperature factor for nitrification at zero degrees")]
    public double[] stf_Nitrif_FctrZero
    {
        get { return TempFactorData_Nitrif.yValueAtZero; }
        set { TempFactorData_Nitrif.yValueAtZero = value; }
    }

    /// <summary>
    /// Curve exponent for calculating the temperature factor for nitrification
    /// </summary>
    [Param]
    [Units("")]
    [Description("Curve exponent for temperature factor")]
    public double[] stf_Nitrif_CvExp
    {
        get { return TempFactorData_Nitrif.CurveExponent; }
        set { TempFactorData_Nitrif.CurveExponent = value; }
    }

    /// <summary>
    /// Parameters to calculate the soil moisture factor for nitrification
    /// </summary>
    private BrokenStickData MoistFactorData_Nitrif = new BrokenStickData();

    /// <summary>
    /// Values of the modified soil water content at which the moisture factor is known
    /// </summary>
    [Param(MinVal = 0.0, MaxVal = 3.0)]
    [Units("0-3")]
    [Description("X values for the moisture factor function")]
    public double[] swf_Nitrif_swx
    {
        get { return MoistFactorData_Nitrif.xVals; }
        set { MoistFactorData_Nitrif.xVals = value; }
    }

    /// <summary>
    /// Values of the moisture factor at given water content
    /// </summary>
    [Param(MinVal = 0.0, MaxVal = 1.0)]
    [Units("0-1")]
    [Description("Y values for the moisture factor function")]
    public double[] swf_Nitrif_y
    {
        get { return MoistFactorData_Nitrif.yVals; }
        set { MoistFactorData_Nitrif.yVals = value; }
    }

    /// <summary>
    /// Parameters to calculate the soil pH factor for nitrification
    /// </summary>
    private BrokenStickData pHFactorData_Nitrif = new BrokenStickData();

    /// <summary>
    /// Values of pH at which factors is known
    /// </summary>
    [Param(MinVal = 0.0, MaxVal = 14.0)]
    [Units("")]
    [Description("X values of pH factor function")]
    public double[] phf_Nitrif_phx
    {
        get { return pHFactorData_Nitrif.xVals; }
        set { pHFactorData_Nitrif.xVals = value; }
    }

    /// <summary>
    /// Values of pH factor at given pH values
    /// </summary>
    [Param(MinVal = 0.0, MaxVal = 1.0)]
    [Units("0-1")]
    [Description("Y values of pH factor function")]
    public double[] phf_Nitrif_y
    {
        get { return pHFactorData_Nitrif.yVals; }
        set { pHFactorData_Nitrif.yVals = value; }
    }

    #endregion factors

    #endregion params for nitrification

    #region Parameters for denitrification and N2O emission processes

    /// <summary>
    /// Denitrification rate coefficient (kg/mg)
    /// </summary>
    [Param(MinVal = 0.0, MaxVal = 1.0)]
    [Units("")]
    [Description("")]
    public double DenitRateCoefficient;

    /// <summary>
    /// Fraction of nitrification lost as denitrification
    /// </summary>
    [Param(MinVal = 0.0, MaxVal = 1.0)]
    [Units("")]
    [Description("")]
    public double n2oLossFactor;

    /// <summary>
    /// Parameter k1 from Thorburn et al (2010) for N2O model
    /// </summary>
    [Param(MinVal = 0.0, MaxVal = 100.0)]
    [Units("")]
    [Description("")]
    public double dnit_k1;

    /// <summary>
    /// Parameter A in the N2N2O function
    /// </summary>
    [Param]
    [Units("")]
    [Description("Parameter A in the function computing the N2:N2O ratio")]
    public double N2N2O_parmA;

    /// <summary>
    /// Parameter B in the function computing the N2:N2O ratio
    /// </summary>
    [Param]
    [Units("")]
    [Description("Parameter B in the function computing the N2:N2O ratio")]
    public double N2N2O_parmB;

    #region Limiting factors

    /// <summary>
    /// Parameter A to compute active carbon (for denitrification)
    /// </summary>
    [Param]
    [Units("")]
    [Description("")]
    public double actC_parmA;

    /// <summary>
    /// Parameter B to compute active carbon (for denitrification)
    /// </summary>
    [Param]
    [Units("")]
    [Description("")]
    public double actC_parmB;

    /// <summary>
    /// Parameters to calculate the temperature effect on denitrification
    /// </summary>
    private BentStickData TempFactorData_Denit = new BentStickData();

    /// <summary>
    /// Optimum temperature for denitrification
    /// </summary>
    [Param(MinVal = 5.0, MaxVal = 100.0)]
    [Units("oC")]
    [Description("Optimum temperature for denitrification")]
    public double[] stf_dnit_Topt
    {
        get { return TempFactorData_Denit.xValueForOptimum; }
        set { TempFactorData_Denit.xValueForOptimum = value; }
    }

    /// <summary>
    /// Temperature factor for denitrification at zero degrees
    /// </summary>
    [Param(MinVal = 0.0, MaxVal = 1.0)]
    [Units("0-1")]
    [Description("Temperature factor for denitrification at zero degrees")]
    public double[] stf_dnit_FctrZero
    {
        get { return TempFactorData_Denit.yValueAtZero; }
        set { TempFactorData_Denit.yValueAtZero = value; }
    }

    /// <summary>
    /// Curve exponent for calculating the temperature factor for denitrification
    /// </summary>
    [Param]
    [Units("")]
    [Description("Curve exponent for temperature factor")]
    public double[] stf_dnit_CvExp
    {
        get { return TempFactorData_Denit.CurveExponent; }
        set { TempFactorData_Denit.CurveExponent = value; }
    }

    /// <summary>
    /// Parameters to calculate the soil moisture factor for denitrification
    /// </summary>
    private BrokenStickData MoistFactorData_Denit = new BrokenStickData();

    /// <summary>
    /// Values of modified soil water content at which the moisture factor is known
    /// </summary>
    [Param(MinVal = 0.0, MaxVal = 3.0)]
    [Units("0-3")]
    [Description("X values for the moisture factor function")]
    public double[] swf_dnit_swx
    {
        get { return MoistFactorData_Denit.xVals; }
        set { MoistFactorData_Denit.xVals = value; }
    }

    /// <summary>
    /// Values of the moisture factor at given water content values
    /// </summary>
    [Param(MinVal = 0.0, MaxVal = 1.0)]
    [Units("0-1")]
    [Description("Y values for the moisture factor function")]
    public double[] swf_dnit_y
    {
        get { return MoistFactorData_Denit.yVals; }
        set { MoistFactorData_Denit.yVals = value; }
    }

    /// <summary>
    /// Parameters to calculate the N2:N2O ratio during denitrification
    /// </summary>
    private BrokenStickData WFPSFactorData_Denit = new BrokenStickData();

    /// <summary>
    /// Values of soil water filled pore sapce at which the WFPS factor is known
    /// </summary>
    [Param(MinVal = 0.0, MaxVal = 100.0)]
    [Units("%")]
    [Description("X values for the WFPS factor function")]
    public double[] swpsf_dnit_swpx
    {
        get { return WFPSFactorData_Denit.xVals; }
        set { WFPSFactorData_Denit.xVals = value; }
    }

    /// <summary>
    /// Values of the WFPS factor at given water fille pore space values
    /// </summary>
    [Param(MinVal = 0.0, MaxVal = 1.0)]
    [Units("0-1")]
    [Description("Y values for the WFPS factor function")]
    public double[] swpsf_dnit_y
    {
        get { return WFPSFactorData_Denit.yVals; }
        set { WFPSFactorData_Denit.yVals = value; }
    }

    #endregion factors

    #endregion params for denitrification

    #region Parameters for handling soil loss process

    /// <summary>
    /// Coefficient a of the enrichment equation
    /// </summary>
    [Param()]
    [Units("")]
    [Description("Erosion enrichment coefficient A")]
    public double enr_a_coeff;

    /// <summary>
    /// Coefficient b of the enrichment equation
    /// </summary>
    [Param()]
    [Units("")]
    [Description("Erosion enrichment coefficient B")]
    public double enr_b_coeff;

    #endregion params for soil loss

    #endregion params for initialisation

    #region Parameters that do or may change during simulation

    #region Soil physics data

    /// <summary>
    /// Today's soil water amount (mm)
    /// </summary>
    [Input]
    [Units("mm")]
    [Description("Soil water amount")]
    private double[] sw_dep;

    /// <summary>
    /// Soil albedo (0-1)
    /// </summary>
    [Input]
    [Units("0-1")]
    [Description("Soil albedo")]
    private double salb;

    /// <summary>
    /// Soil temperature (oC), as computed by an external module (SoilTemp)
    /// </summary>
    [Input(IsOptional = true)]
    [Units("oC")]
    [Description("Soil temperature")]
    private double[] ave_soil_temp;

    #endregion physiscs data

    #region Soil pH data

    /// <summary>
    /// pH of soil (assumed equivalent to a 1:1 soil-water slurry)
    /// </summary>
    [Param(IsOptional = true, MinVal = 3.5, MaxVal = 11.0)]
    [Input(IsOptional = true)]
    [Description("Soil pH")]
    public double[] ph;

    #endregion ph data

    #region Values for soil organic matter (som)

    /// <summary>
    /// Total soil organic carbon content (%)
    /// </summary>
    [Param]
    [Output]
    [Units("%")]
    [Description("Soil organic carbon (exclude FOM)")]
    public double[] oc
    {
        get
        {
            double[] result;
            if (initDone)
            {
                result = new double[dlayer.Length];
                for (int layer = 0; layer < dlayer.Length; ++layer)
                    result[layer] = (hum_c[layer] + biom_c[layer]) * convFactor[layer] / 10000;  // (100/1000000) = convert to ppm and then to %
            }
            else
            {
                // no value has been asigned yet, return null
                result = reset_oc;
            }
            return result;
        }
        set
        {
            if (initDone)
            {
                Console.WriteLine(" Attempt to assign values for OC during simulation, "
                                 + "this operation is not valid and will be ignored");
            }
            else
            {
                // Store initial values, check and initialisation of C pools is done on InitCalc().
                reset_oc = value;
                // these values are also used OnReset
            }
        }
    }

    #endregion soil organic matter data

    #region Values for soil mineral N

    /// <summary>
    /// Soil urea nitrogen content (ppm)
    /// </summary>
    [Param(IsOptional = true, MinVal = 0.0, MaxVal = 10000.0)]
    [Output]
    [Units("mg/kg")]
    [Description("Soil urea nitrogen content")]
    public double[] ureappm
    {
        get
        {
            double[] result;
            if (initDone)
            {
                result = new double[dlayer.Length];
                for (int layer = 0; layer < dlayer.Length; ++layer)
                    result[layer] = _urea[layer] * convFactor[layer];
            }
            else
                result = reset_ureappm;
            return result;
        }
        set
        {
            if (initDone)
            { // should this be disallowed/restricted?? changes should be done via NitrogenChanged

                double sumOld = MathUtility.Sum(_urea);      // original amount

                for (int layer = 0; layer < value.Length; ++layer)
                    _urea[layer] = MathUtility.Divide(value[layer], convFactor[layer], 0.0);       //Convert from ppm to kg/ha

                if (!isResetting)
                    SendExternalMassFlowN(MathUtility.Sum(_urea) - sumOld);
            }
            else
                reset_ureappm = value;      // check is done on InitCalc
        }
    }

    /// <summary>
    /// Soil ammonium nitrogen content (ppm)
    /// </summary>
    [Param(IsOptional = true, MinVal = 0.0, MaxVal = 10000.0)]
    [Output]
    [Units("mg/kg")]
    [Description("Soil ammonium nitrogen content")]
    public double[] nh4ppm
    {
        get
        {
            double[] result;
            if (initDone)
            {
                result = new double[dlayer.Length];
                for (int layer = 0; layer < dlayer.Length; ++layer)
                    result[layer] = _nh4[layer] * convFactor[layer];
            }
            else
                result = reset_nh4ppm;
            return result;
        }
        set
        {
            if (initDone)
            {
                double sumOld = MathUtility.Sum(_nh4);   // original values

                for (int layer = 0; layer < value.Length; ++layer)
                    _nh4[layer] = MathUtility.Divide(value[layer], convFactor[layer], 0.0);       //Convert from ppm to kg/ha

                if (!isResetting)
                    SendExternalMassFlowN(MathUtility.Sum(_nh4) - sumOld);
            }
            else
                reset_nh4ppm = value;
        }
    }

    /// <summary>
    /// Soil nitrate nitrogen content (ppm)
    /// </summary>
    [Param(IsOptional = true, MinVal = 0.0, MaxVal = 10000.0)]
    [Output]
    [Units("mg/kg")]
    [Description("Soil nitrate nitrogen content")]
    public double[] no3ppm
    {
        get
        {
            double[] result;
            if (initDone)
            {
                result = new double[dlayer.Length];
                for (int layer = 0; layer < dlayer.Length; ++layer)
                    result[layer] = _no3[layer] * convFactor[layer];
            }
            else
                result = reset_no3ppm;
            return result;
        }
        set
        {
            if (initDone)
            {
                double sumOld = MathUtility.Sum(_no3);   // original values
                for (int layer = 0; layer < value.Length; ++layer)
                    _no3[layer] = MathUtility.Divide(value[layer], convFactor[layer], 0.0);       //Convert from ppm to kg/ha

                if (!isResetting)
                    SendExternalMassFlowN(MathUtility.Sum(_no3) - sumOld);
            }
            else
                reset_no3ppm = value;
        }
    }

    #endregion  mineral N data

    #region Soil loss data

    // it is assumed any changes in soil profile are due to erosion
    // this should be done via an event (RCichota)

    /// <summary>
    /// 
    /// </summary>
    [Output]
    [Description("Define whether soil profile reduction is on")]
    private string n_reduction
    { set { ProfileReductionAllowed = value.StartsWith("on"); } }

    /// <summary>
    /// Soil loss due to erosion (t/ha)
    /// </summary>
    [Input(IsOptional = true)]
    [Units("t/ha")]
    [Description("Soil loss due to erosion")]
    private double soil_loss;

    #endregion

    #region Pond data

    /// <summary>
    /// Indicates whether pond is active or not
    /// </summary>
    /// <remarks>
    /// If there is a pond, the decomposition of surface OM will be done by that model
    /// </remarks>
    private bool isPondActive = false;
    [Input(IsOptional = true)]
    [Description("Indicates whether pond is active or not")]
    private string pond_active
    { set { isPondActive = (value == "yes"); } }

    /// <summary>
    /// Amount of C decomposed in pond that is added to soil m. biomass
    /// </summary>
    [Input(IsOptional = true)]
    [Units("kg/ha")]
    [Description("Amount of C decomposed in pond being acced to Biom")]
    private double pond_biom_C;

    /// <summary>
    /// Amount of C decomposed in pond that is added to soil humus
    /// </summary>	
    [Input(IsOptional = true)]
    [Units("kg/ha")]
    [Description("Amount of C decomposed in pond being acced to Humus")]
    private double pond_hum_C;

    #endregion

    #region Inhibitors data

    /// <summary>
    /// Factor reducing nitrification due to the presence of a inhibitor
    /// </summary>
    private double[] InhibitionFactor_Nitrification = null;
    [Input(IsOptional = true)]
    [Units("0-1")]
    [Description("Factor reducing nitrification rate")]
    private double[] nitrification_inhibition
    {
        set
        {
            if (initDone)
            {
                for (int layer = 0; layer < dlayer.Length; layer++)
                {
                    if (layer < value.Length)
                    {
                        InhibitionFactor_Nitrification[layer] = value[layer];
                        if (InhibitionFactor_Nitrification[layer] < -epsilon)
                        {
                            InhibitionFactor_Nitrification[layer] = 0.0;
                            Console.WriteLine("Value for nitrification inhibition is below lower limit, value will be adjusted to 0.0");
                        }
                        else if (InhibitionFactor_Nitrification[layer] > 1.0)
                        {
                            InhibitionFactor_Nitrification[layer] = 1.0;
                            Console.WriteLine("Value for nitrification inhibition is above upper limit, value will be adjusted to 1.0");
                        }
                    }
                    else
                        InhibitionFactor_Nitrification[layer] = 0.0;
                }
            }
        }
    }

    #endregion

    #endregion params that may change

    #endregion

    #region Outputs we make available to other components

    #region Outputs for Nitrogen

    #region Changes for today - deltas

    /// <summary>
    /// N carried out in sediment via runoff/erosion
    /// </summary>
    [Output]
    [Units("kg")]
    [Description("N loss carried in sediment")]
    private double dlt_n_loss_in_sed;

    /// <summary>
    /// Net nh4 change today
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("Net NH4 change today")]
    private double[] dlt_nh4_net
    {
        get
        {
            double[] result = new double[dlayer.Length];
            for (int layer = 0; layer < dlayer.Length; layer++)
                result[layer] = _nh4[layer] - TodaysInitialNH4[layer];
            return result;
        }
    }

    /// <summary>
    /// Net NH4 transformation today
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("Net NH4 transformation")]
    private double[] nh4_transform_net
    {
        get
        {
            double[] result = new double[dlayer.Length];
            for (int layer = 0; layer < dlayer.Length; layer++)
                result[layer] = dlt_res_nh4_min[layer] +
                                dlt_fom_n_min[layer] +
                                dlt_biom_n_min[layer] +
                                dlt_hum_n_min[layer] -
                                dlt_nitrification[layer] +
                                dlt_urea_hydrolysis[layer] +
                                nh4_deficit_immob[layer];
            return result;
        }
    }

    /// <summary>
    /// Net no3 change today
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("Net NO3 change today")]
    private double[] dlt_no3_net
    {
        get
        {
            double[] result = new double[dlayer.Length];
            for (int layer = 0; layer < dlayer.Length; layer++)
                result[layer] = _no3[layer] - TodaysInitialNO3[layer];
            return result;
        }
    }

    /// <summary>
    /// Net NO3 transformation today
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("Net NO3 transformation")]
    private double[] no3_transform_net
    {
        get
        {
            double[] result = new double[dlayer.Length];
            for (int layer = 0; layer < dlayer.Length; layer++)
                result[layer] = dlt_res_no3_min[layer] -
                                dlt_no3_dnit[layer] +
                                dlt_nitrification[layer] -
                                dlt_nh4_dnit[layer] -
                                nh4_deficit_immob[layer];
            return result;
        }
    }

    /// <summary>
    /// Net mineralisation today
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("Net N mineralised in soil")]
    private double[] dlt_n_min
    {
        get
        {
            double[] result = new double[dlayer.Length];
            for (int layer = 0; layer < dlayer.Length; layer++)
                result[layer] += dlt_hum_n_min[layer] + dlt_biom_n_min[layer] + dlt_fom_n_min[layer];
            return result;
        }
    }

    /// <summary>
    /// Net N mineralisation from residue decomposition
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("Net N mineralisation from residue decomposition")]
    private double[] dlt_n_min_res
    {
        get
        {
            double[] result = new double[dlayer.Length];
            for (int layer = 0; layer < dlayer.Length; layer++)
                result[layer] += dlt_res_no3_min[layer] + dlt_res_nh4_min[layer];
            return result;
        }
    }

    /// <summary>
    /// Net NH4 mineralisation from residue decomposition
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("Net convertion of NH4 for residue mineralisation/immobilisation")]
    private double[] dlt_res_nh4_min;

    /// <summary>
    /// Net NO3 mineralisation from residue decomposition
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("Net convertion of NO3 for residue mineralisation/immobilisation")]
    private double[] dlt_res_no3_min;

    /// <summary>
    /// Amount of N converted from each FOM pool
    /// </summary>
    private double[][] dlt_n_fom = new double[3][];

    /// <summary>
    /// Net FOM N mineralised (negative for immobilisation)
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("Net FOM N mineralised, negative for immobilisation")]
    private double[] dlt_fom_n_min;

    /// <summary>
    /// Net N mineralised for humic pool
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("Net humic N mineralised, negative for immobilisation")]
    private double[] dlt_hum_n_min;

    /// <summary>
    /// Net N mineralised from m. biomass pool
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("Net biomass N mineralised")]
    private double[] dlt_biom_n_min;

    /// <summary>
    /// Total net N mineralised (residues plus soil OM)
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("Total net N mineralised (soil plus residues)")]
    private double[] dlt_n_min_tot
    {
        get
        {
            double[] result = new double[dlayer.Length];
            for (int layer = 0; layer < dlayer.Length; layer++)
                result[layer] += dlt_hum_n_min[layer] +
                                 dlt_biom_n_min[layer] +
                                 dlt_fom_n_min[layer] +
                                 dlt_res_no3_min[layer] +
                                 dlt_res_nh4_min[layer];
            return result;
        }
    }

    /// <summary>
    /// Nitrogen coverted by hydrolisys (urea into NH4)
    /// </summary>
    private double[] dlt_urea_hydrolysis;
    /// <summary>
    /// Nitrogen coverted by hydrolysis (from urea to NH4)
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("Nitrogen coverted by hydrolysis")]
    private double[] dlt_urea_hydrol
    { get { return dlt_urea_hydrolysis; } }

    /// <summary>
    /// Nitrogen coverted by nitrification (NH4 into NO3)
    /// </summary>
    private double[] dlt_nitrification;
    /// <summary>
    /// Nitrogen coverted by nitrification (from NH4 to either NO3 or N2O)
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("Nitrogen coverted by nitrification")]
    private double[] nitrification
    { get { return dlt_nitrification; } }

    /// <summary>
    /// Effective, or net, nitrogen coverted by nitrification (from NH4 to NO3)
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("Effective nitrogen coverted by nitrification")]
    private double[] effective_nitrification
    {
        get
        {
            double[] result = new double[dlayer.Length];
            for (int layer = 0; layer < dlayer.Length; ++layer)
                result[layer] = dlt_nitrification[layer] - dlt_nh4_dnit[layer];
            return result;
        }
    }

    /// <summary>
    /// N2O N produced during nitrification
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("N2O N produced during nitrification")]
    private double[] dlt_n2o_nitrif;

    /// <summary>
    /// N2O N produced during nitrification
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("NH4 N denitrified")]
    private double[] dlt_nh4_dnit
    { get { return dlt_n2o_nitrif; } }

    /// <summary>
    /// NO3 N denitrified
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("NO3 N denitrified")]
    private double[] dlt_no3_dnit;

    /// <summary>
    /// N2O N produced during denitrification
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("N2O N produced during denitrification")]
    private double[] dlt_n2o_dnit;

    /// <summary>
    /// Total N2O amount produced today
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("Amount of N2O produced")]
    private double[] n2o_atm
    {
        get
        {
            double[] result = new double[dlayer.Length];
            for (int layer = 0; layer < dlayer.Length; ++layer)
                result[layer] = dlt_n2o_dnit[layer] + dlt_n2o_nitrif[layer];
            return result;
        }
    }

    /// <summary>
    /// Amount of N2 produced
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("Amount of N2 produced")]
    private double[] n2_atm
    {
        get
        {
            double[] result = new double[dlayer.Length];
            for (int layer = 0; layer < dlayer.Length; ++layer)
                result[layer] = dnit[layer] - (dlt_n2o_dnit[layer] + dlt_n2o_nitrif[layer]);
            return result;
        }
    }

    /// <summary>
    /// N converted by denitrification
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("N converted by denitrification")]
    private double[] dnit
    {
        get
        {
            double[] result = new double[dlayer.Length];
            for (int layer = 0; layer < dlayer.Length; layer++)
                result[layer] = dlt_no3_dnit[layer] + dlt_nh4_dnit[layer];
            return result;
        }
    }

    /// <summary>
    /// Excess N required above NH4 supply (for immobilisation)
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("NH4 deficit for immobilisation")]
    private double[] nh4_deficit_immob;

    #endregion deltas

    #region Amounts in solute forms

    /// <summary>
    /// Soil urea nitrogen amount (kgN/ha)
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("Soil urea nitrogen amount")]
    public double[] urea
    {
        get
        {
            return _urea;
        }
        set
        { // should this be disallowed?? changes should be done via NitrogenChanged, initialisation uses ureappm
            double sumOld = MathUtility.Sum(_urea);
            for (int layer = 0; layer < dlayer.Length; ++layer)
            {
                if (layer >= _urea.Length)
                {
                    Console.WriteLine(" Attempt to assign urea value to a non-existent soil layer - extra values will be ignored");
                    break;
                }
                else if (layer >= value.Length)
                {
                    // not all values were supplied, assume minimum
                    Array.Resize(ref value, value.Length + 1);
                    value[layer] = 0.0;
                }
                else
                {
                    // a value was supplied, check whether it is valid (positive and within bounds)
                    bool IsVariableOK = (value[layer] >= 0.0 || value[layer] <= 10000);
                    if (!IsVariableOK)
                        value[layer] = 0.0;
                }
                _urea[layer] = value[layer];
            }

            SendExternalMassFlowN(MathUtility.Sum(_urea) - sumOld);
        }
    }

    /// <summary>
    /// Soil ammonium nitrogen amount (kgN/ha)
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("Soil ammonium nitrogen amount")]
    public double[] nh4
    {
        get
        {
            return _nh4;
        }
        set
        { // should this be disallowed?? changes should be done via NitrogenChanged
            double sumOld = MathUtility.Sum(_nh4);
            for (int layer = 0; layer < dlayer.Length; ++layer)
            {
                if (layer >= _nh4.Length)
                {
                    Console.WriteLine(" Attempt to assign NH4 value to a non-existent soil layer - extra values will be ignored");
                    break;
                }
                else if (layer >= value.Length)
                {
                    // not all values were supplied, assume minimum
                    Array.Resize(ref value, value.Length + 1);
                    value[layer] = 0.0;
                }
                else
                {
                    // a value was supplied, check whether it is valid (positive and within bounds)
                    bool IsVariableOK = (value[layer] >= 0.0 || value[layer] <= 10000);
                    if (!IsVariableOK)
                        value[layer] = 0.0;
                }
                _nh4[layer] = value[layer];
            }
            SendExternalMassFlowN(MathUtility.Sum(_nh4) - sumOld);
        }
    }

    /// <summary>
    /// Soil nitrate nitrogen amount (kgN/ha)
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("Soil nitrate nitrogen amount")]
    public double[] no3
    {
        get
        {
            return _no3;
        }
        set
        { // should this be disallowed?? changes should be done via NitrogenChanged
            double sumOld = MathUtility.Sum(_no3);
            for (int layer = 0; layer < dlayer.Length; ++layer)
            {
                if (layer >= _no3.Length)
                {
                    Console.WriteLine(" Attempt to assign NO3 value to a non-existent soil layer - extra values will be ignored");
                    break;
                }
                else if (layer >= value.Length)
                {
                    // not all values were supplied, assume minimum
                    Array.Resize(ref value, value.Length + 1);
                    value[layer] = 0.0;
                }
                else
                {
                    // a value was supplied, check whether it is valid (positive and within bounds)
                    bool IsVariableOK = (value[layer] >= 0.0 || value[layer] <= 10000);
                    if (!IsVariableOK)
                        value[layer] = 0.0;
                }
                _no3[layer] = value[layer];
            }
            SendExternalMassFlowN(MathUtility.Sum(_no3) - sumOld);

        }
    }

    #endregion

    #region Amounts in various pools

    /// <summary>
    /// Total nitrogen in FOM
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("Nitrogen in FOM")]
    private double[] fom_n
    {
        get
        {
            double[] result = new double[dlayer.Length];
            for (int layer = 0; layer < dlayer.Length; layer++)
                for (int pool = 0; pool < 3; pool++)
                    result[layer] += fom_n_pool[pool][layer];
            return result;
        }
    }

    /// <summary>
    /// Nitrogen in FOM pool 1
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("Nitrogen in FOM pool 1")]
    private double[] fom_n_pool1
    { get { return fom_n_pool[0]; } }

    /// <summary>
    /// Nitrogen in FOM pool 2
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("Nitrogen in FOM pool 2")]
    private double[] fom_n_pool2
    { get { return fom_n_pool[1]; } }

    /// <summary>
    /// Nitrogen in FOM pool 3
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("Nitrogen in FOM pool 3")]
    private double[] fom_n_pool3
    { get { return fom_n_pool[2]; } }

    /// <summary>
    /// Soil humic N
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("Soil humic nitrogen")]
    private double[] hum_n;

    /// <summary>
    /// Soil biomass nitrogen
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("Soil biomass nitrogen")]
    private double[] biom_n;

    /// <summary>
    /// Total N in soil
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("Total N in soil")]
    private double[] nit_tot
    {
        get
        {
            double[] result = null;
            if (dlayer != null)
            {
                result = new double[dlayer.Length];
                for (int layer = 0; layer < dlayer.Length; ++layer)
                    result[layer] = fom_n[layer] +
                                    hum_n[layer] +
                                    biom_n[layer] +
                                    _no3[layer] +
                                    _nh4[layer] +
                                    _urea[layer];
            }
            return result;
        }
    }

    #endregion

    #region Nitrogen balance

    /// <summary>
    /// Balance of nitrogen: deltaN - losses
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("Nitrogen balance")]
    private double nitrogenbalance
    {
        get
        {
            double deltaN = SumDoubleArray(nit_tot) - TodaysInitialN;  // variation in N today
            double losses = SumDoubleArray(dlt_nh4_dnit) + SumDoubleArray(dlt_no3_dnit);
            return -(losses + deltaN);
            // why leaching losses are not accounted and what about the inputs?
        }
    }

    #endregion

    #endregion

    #region Outputs for Carbon

    #region General values

    /// <summary>
    /// Number of fom types read - is this really needed?
    /// </summary>
    [Output]
    [Description("Number of FOM types")]
    private int num_fom_types
    { get { return fom_types.Length; } }

    /// <summary>
    /// Carbohydrate fraction of FOM (0-1)
    /// </summary>
    [Output]
    [Units("0-1")]
    [Description("Fraction of carbohydrate in FOM")]
    private double fr_carb
    { get { return fract_carb[fom_type]; } }

    /// <summary>
    /// Cellulose fraction of FOM (0-1)
    /// </summary>
    [Output]
    [Units("0-1")]
    [Description("Fraction of cellulose in FOM")]
    private double fr_cell
    { get { return fract_cell[fom_type]; } }

    /// <summary>
    /// Lignin fraction of FOM (0-1)
    /// </summary>
    [Output]
    [Units("0-1")]
    [Description("Fraction of lignin in FOM")]
    private double fr_lign
    { get { return fract_lign[fom_type]; } }

    #endregion

    #region Changes for today - deltas

    /// <summary>
    /// Carbon loss in sediment, via runoff/erosion
    /// </summary>
    [Output]
    [Units("kg")]
    [Description("Carbon loss in sediment")]
    private double dlt_c_loss_in_sed;

    /// <summary>
    /// Amount of C from each FOM pool converted into humus
    /// </summary>
    private double[][] dlt_c_fom_to_hum = new double[3][];
    /// <summary>
    /// Amount of C converted from FOM to humic (kg/ha)
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("FOM C converted to humic")]
    private double[] dlt_fom_c_hum
    {
        get
        {
            double[] result = new double[dlayer.Length];
            for (int layer = 0; layer < dlayer.Length; ++layer)
                for (int fractn = 0; fractn < 3; fractn++)
                    result[layer] += dlt_c_fom_to_hum[fractn][layer];
            return result;
        }
    }

    /// <summary>
    /// Amount of C from each FOM pool converted into m. biomass
    /// </summary>
    private double[][] dlt_c_fom_to_biom = new double[3][];
    /// <summary>
    /// Amount of C converted from FOM to m. biomass (kg/ha)
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("FOM C converted to biomass")]
    private double[] dlt_fom_c_biom
    {
        get
        {
            double[] result = new double[dlayer.Length];
            for (int layer = 0; layer < dlayer.Length; ++layer)
                for (int fractn = 0; fractn < 3; fractn++)
                    result[layer] += dlt_c_fom_to_biom[fractn][layer];
            return result;
        }
    }

    /// <summary>
    /// Amount of C from each FOM pool lost to the atmosphere
    /// </summary>
    private double[][] dlt_c_fom_to_atm = new double[3][];
    /// <summary>
    /// Amount of C lost to atmosphere from FOM
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("FOM C lost to atmosphere")]
    private double[] dlt_fom_c_atm
    {
        get
        {
            double[] result = new double[dlayer.Length];
            for (int layer = 0; layer < dlayer.Length; ++layer)
                for (int fractn = 0; fractn < 3; fractn++)
                    result[layer] += dlt_c_fom_to_atm[fractn][layer];
            return result;
        }
    }

    /// <summary>
    /// Humic C converted to biomass
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("Humic C converted to biomass")]
    private double[] dlt_hum_c_biom;

    /// <summary>
    /// Humic C lost to atmosphere
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("Humic C lost to atmosphere")]
    private double[] dlt_hum_c_atm;

    /// <summary>
    /// Biomass C converted to humic
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("Biomass C converted to humic")]
    private double[] dlt_biom_c_hum;

    /// <summary>
    /// Biomass C lost to atmosphere
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("Biomass C lost to atmosphere")]
    private double[] dlt_biom_c_atm;

    /// <summary>
    /// Carbon from residues converted to biomass (kg/ha)
    /// </summary>
    public double[] dlt_c_res_to_biom;
    /// <summary>
    /// Carbon from residues converted to biomass C
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("Carbon from residues converted to biomass")]
    private double[] dlt_res_c_biom
    { get { return dlt_c_res_to_biom; } }

    /// <summary>
    /// Carbon from residues converted to humus (kg/ha)
    /// </summary>
    public double[] dlt_c_res_to_hum;
    /// <summary>
    /// Carbon from residues converted to humic C
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("Carbon from residues converted to humus")]
    private double[] dlt_res_c_hum
    { get { return dlt_c_res_to_hum; } }

    /// <summary>
    /// Carbon from residues lost to atmosphere during decomposition (kg/ha)
    /// </summary>
    public double[] dlt_c_res_to_atm;
    /// <summary>
    /// Carbon from residues lost to atmosphere
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("Carbon from residues lost to atmosphere during decomposition")]
    private double[] dlt_res_c_atm
    { get { return dlt_c_res_to_atm; } }

    /// <summary>
    /// Delta C in pool 1 of FOM - needed by SoilP
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("Delta FOM C pool in fraction 1")]
    private double[] dlt_fom_c_pool1
    {
        get
        {
            double[] result = new double[dlayer.Length];
            for (int layer = 0; layer < dlayer.Length; layer++)
                result[layer] = dlt_c_fom_to_hum[0][layer] + dlt_c_fom_to_biom[0][layer] + dlt_c_fom_to_atm[0][layer];
            return result;
        }
    }

    /// <summary>
    /// Delta C in pool 2 of FOM - needed by SoilP
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("Delta FOM C pool in fraction 2")]
    private double[] dlt_fom_c_pool2
    {
        get
        {
            double[] result = new double[dlayer.Length];
            for (int layer = 0; layer < dlayer.Length; layer++)
                result[layer] = dlt_c_fom_to_hum[1][layer] + dlt_c_fom_to_biom[1][layer] + dlt_c_fom_to_atm[1][layer];
            return result;
        }
    }

    /// <summary>
    /// Delta C in pool 3 of FOM - needed by SoilP
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("Delta FOM C pool in fraction 3")]
    private double[] dlt_fom_c_pool3
    {
        get
        {
            double[] result = new double[dlayer.Length];
            for (int layer = 0; layer < dlayer.Length; layer++)
                result[layer] = dlt_c_fom_to_hum[2][layer] + dlt_c_fom_to_biom[2][layer] + dlt_c_fom_to_atm[2][layer];
            return result;
        }
    }

    /// <summary>
    /// Carbon from all residues to m. biomass
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("Carbon from all residues to biomass")]
    private double[] soilp_dlt_res_c_biom
    { get { return dlt_c_res_to_biom; } }

    /// <summary>
    /// Carbon from all residues to humic pool
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("Carbon from all residues to humic")]
    private double[] soilp_dlt_res_c_hum
    { get { return dlt_c_res_to_hum; } }

    /// <summary>
    /// Carbon lost from all residues to atmosphere
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("Carbon from all residues to atmosphere")]
    private double[] soilp_dlt_res_c_atm
    { get { return dlt_c_res_to_atm; } }

    /// <summary>
    /// Total CO2 amount produced today
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("Amount of co2 produced in the soil")]
    private double[] co2_atm
    {
        get
        {
            double[] result = new double[dlayer.Length];
            for (int layer = 0; layer < dlayer.Length; layer++)
                result[layer] = dlt_c_fom_to_atm[0][layer] +
                                dlt_c_fom_to_atm[1][layer] +
                                dlt_c_fom_to_atm[2][layer] +
                                dlt_biom_c_atm[layer] +
                                dlt_hum_c_atm[layer];
            return result;
        }
    }

    #endregion

    #region Amounts in various pools

    /// <summary>
    /// Fresh organic C - FOM
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("Soil FOM C")]
    private double[] fom_c
    {
        get
        {
            double[] result = new double[dlayer.Length];
            for (int layer = 0; layer < dlayer.Length; layer++)
                for (int pool = 0; pool < 3; pool++)
                    result[layer] += fom_c_pool[pool][layer];
            return result;
        }
    }

    /// <summary>
    /// Amount of C in pool 1 of FOM
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("FOM C in pool 1")]
    private double[] fom_c_pool1
    { get { return fom_c_pool[0]; } }

    /// <summary>
    /// Amount of C in pool 2 of FOM
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("FOM C in pool 2")]
    private double[] fom_c_pool2
    { get { return fom_c_pool[1]; } }

    /// <summary>
    /// Amount of C in pool 3 of FOM
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("FOM C in pool 3")]
    private double[] fom_c_pool3
    { get { return fom_c_pool[2]; } }

    /// <summary>
    /// Amount of C in humic pool
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("Soil humic C")]
    private double[] hum_c;

    /// <summary>
    /// Amount of C in inert humic pool
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("Soil humic inert C")]
    private double[] inert_c;

    /// <summary>
    /// Amount of C in m. biomass pool
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("Soil biomass C")]
    private double[] biom_c;

    /// <summary>
    /// Total carbon amount in the soil
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("Total soil carbon")]
    private double[] carbon_tot
    {
        get
        {
            double[] result = null;
            if (dlayer != null)
            {
                result = new double[dlayer.Length];
                for (int layer = 0; layer < dlayer.Length; ++layer)
                    result[layer] = fom_c_pool[0][layer] +
                                    fom_c_pool[1][layer] +
                                    fom_c_pool[2][layer] +
                                    hum_c[layer] +
                                    biom_c[layer];
            }
            return result;
        }
    }

    #endregion

    #region Carbon Balance

    /// <summary>
    /// Balance of C in soil: deltaC - losses
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("Carbon balance")]
    private double carbonbalance
    {
        get
        {
            double deltaC = SumDoubleArray(carbon_tot) - TodaysInitialC;     // variation in C today
            double losses = SumDoubleArray(dlt_c_res_to_atm)
                          + SumDoubleArray(dlt_c_fom_to_atm[0])
                          + SumDoubleArray(dlt_c_fom_to_atm[1])
                          + SumDoubleArray(dlt_c_fom_to_atm[2])
                          + SumDoubleArray(dlt_hum_c_atm)
                          + SumDoubleArray(dlt_biom_c_atm);
            return -(losses + deltaC);
        }
    }

    #endregion

    #endregion

    #region Factors and other outputs

    /// <summary>
    /// amount of P coverted by residue mineralisation
    /// </summary>
    [Output]
    [Units("kg/ha")]
    [Description("P coverted by residue mineralisation (needed by SoilP)")]
    private double[] soilp_dlt_org_p;

    /// <summary>
    /// Soil temperature (oC), values actually used in the model
    /// </summary>
    private double[] Tsoil;

    /// <summary>
    /// SoilN's simple soil temperature
    /// </summary>
    [Output]
    [Units("oC")]
    [Description("Soil temperature")]
    private double[] st
    {
        get
        {
            double[] result = new double[0];
            // if (usingSimpleSoilTemp)   // this should limit the output to only the variable calculated here. However the plant modules still look for 'st' instead of 'ave_soil_temp'
            //    result = Tsoil;
            return Tsoil;
        }
    }

    #endregion

    #endregion  outputs

    #region Useful constants

    /// <summary>
    /// Value to evaluate precision against floating point variables
    /// </summary>
    private readonly double epsilon = 0.000000000001;
    //private double epsilon = Math.Pow(2, -24);


    /// <summary>
    /// Threshold value to trigger a fatal error when very large values are detected (for C)
    /// </summary>
    public double WarningThresholdForDltC = 100;

    /// <summary>
    /// Threshold value to trigger a fatal error when very large values are detected (for N)
    /// </summary>
    public double WarningThresholdForDltN = 10;

    #endregion constants

    #region Internal variables

    #region Components

    /// <summary>
    /// The SoilN internal soil temperature module - to be avoided (deprecated)
    /// </summary>
    private simpleSoilTemp simpleST;

    #endregion components

    #region Soil physics data

    /// <summary>
    /// The soil layer thickness at the start of the simulation
    /// </summary>
    private double[] reset_dlayer;

    /// <summary>
    /// Soil layers' thichness (mm)
    /// </summary>
    private double[] dlayer;

    /// <summary>
    /// Soil bulk density for each layer (g/cm3)
    /// </summary>
    private double[] SoilDensity;

    /// <summary>
    /// Soil water amount at saturation (mm)
    /// </summary>
    private double[] sat_dep;

    /// <summary>
    /// Soil water amount at drainage upper limit (mm)
    /// </summary>
    private double[] dul_dep;

    /// <summary>
    /// Soil water amount at drainage lower limit (mm)
    /// </summary>
    private double[] ll15_dep;

    #endregion

    #region Initial C and N amounts

    /// <summary>
    /// The initial OC content for each layer of the soil (%). Also used onReset
    /// </summary>
    private double[] reset_oc;

    /// <summary>
    /// Initial content of urea in each soil layer (ppm). Also used onReset
    /// </summary>
    private double[] reset_ureappm;

    /// <summary>
    /// Initial content of NH4 in each soil layer (ppm). Also used onReset
    /// </summary>
    private double[] reset_nh4ppm;

    /// <summary>
    /// Initial content of NO3 in each soil layer (ppm). Also used onReset
    /// </summary>
    private double[] reset_no3ppm;

    #endregion

    #region Mineral N amounts

    /// <summary>
    /// Internal variable holding the urea amounts
    /// </summary>
    private double[] _urea;

    /// <summary>
    /// Internal variable holding the nh4 amounts
    /// </summary>
    private double[] _nh4;

    /// <summary>
    /// Internal variable holding the no3 amounts
    /// </summary>
    private double[] _no3 = null;

    #endregion

    #region Organic C and N amounts

    /// <summary>
    /// Carbon amount in FOM pools
    /// </summary>
    private double[][] fom_c_pool = new double[3][];

    /// <summary>
    /// Nitrogen amount in FOM pools
    /// </summary>
    private double[][] fom_n_pool = new double[3][];

    #endregion

    #region Deltas in mineral nitrogen

    /// <summary>
    /// Variations in urea as given by another component
    /// </summary>
    /// <remarks>
    /// This property checks changes in the amount of urea at each soil layer
    ///  - If values are not supplied for all layers, these will be assumed zero
    ///  - If values are supplied in excess, these will ignored
    ///  - Each value is tested whether it is within bounds, then it is added to the actual amount, this amount is then tested for its bounds
    /// </remarks>
    private double[] dlt_urea
    {
        set
        {
            for (int layer = 0; layer < value.Length; ++layer)
            {
                if (layer < dlayer.Length)
                {
                    // update variable and check its value
                    _urea[layer] += value[layer];
                    CheckNegativeValues(ref _urea[layer], layer, "urea", "deltaUrea");
                }

                else
                {
                    string myMessage = " Attempt to change the Urea value of a non-existent layer - extra values will be ignored";
                    writeMessage(myMessage);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Variations in nh4 as given by another component
    /// </summary>
    private double[] dlt_nh4
    {
        set
        {
            for (int layer = 0; layer < value.Length; ++layer)
            {
                if (layer < dlayer.Length)
                {
                    // update variable and check its value
                    _nh4[layer] += value[layer];
                    CheckNegativeValues(ref _nh4[layer], layer, "nh4", "deltaNH4");
                }

                else
                {
                    Console.WriteLine(" Attempt to change the NH4 value of a non-existent layer - extra values will be ignored");
                    break;

                }
            }
        }
    }

    /// <summary>
    /// Variations in no3 as given by another component
    /// </summary>
    private double[] dlt_no3
    {
        set
        {
            for (int layer = 0; layer < value.Length; ++layer)
            {
                if (layer < dlayer.Length)
                {
                    // update variable and check its value
                    _no3[layer] += value[layer];
                    CheckNegativeValues(ref _no3[layer], layer, "no3", "deltaNO3");
                }
                else
                {
                    Console.WriteLine(" Attempt to change the NO3 value of a non-existent layer - extra values will be ignored");
                    break;
                }
            }
        }
    }

    #endregion

    #region Decision auxiliary variables

    /// <summary>
    /// Marker for whether initialisation has been finished or not
    /// </summary>
    private bool initDone = false;

    /// <summary>
    /// Marker for whether a reset is going on
    /// </summary>
    private bool isResetting = false;

    /// <summary>
    /// Marker for whether external soil temperature is supplied, otherwise use internal
    /// </summary>
    private bool usingSimpleSoilTemp = false;

    /// <summary>
    /// Marker for whether external ph is supplied, otherwise default is used
    /// </summary>
    private bool usingSimpleSoilpH = false;

    #endregion

    #region Miscelaneous

    /// <summary>
    /// Total C content at the beginning of the day
    /// </summary>
    private double TodaysInitialC;

    /// <summary>
    /// Total N content at the beginning of the day
    /// </summary>
    private double TodaysInitialN;

    /// <summary>
    /// Amount of  N as NH4 at the beginning of the day (kg/ha)
    /// </summary>
    private double[] TodaysInitialNH4;

    /// <summary>
    /// Amount of  N as NO3 at the beginning of the day (kg/ha)
    /// </summary>
    private double[] TodaysInitialNO3;

    /// <summary>
    /// The initial FOM distribution, a 0-1 fraction for each layer
    /// </summary>
    private double[] FOMiniFraction;

    /// <summary>
    /// Type of FOM being used
    /// </summary>
    private int fom_type;

    /// <summary>
    /// The C:N ratio of each fom pool
    /// </summary>
    private double[] fomPoolsCNratio;

    /// <summary>
    /// Factor for converting kg/ha into ppm
    /// </summary>
    private double[] convFactor;

    /// <summary>
    /// Name of residues decomposing
    /// </summary>
    private string[] residueName;

    /// <summary>
    /// Type of decomposing residue
    /// </summary>
    private string[] residueType;

    /// <summary>
    /// Potential residue C decomposition (kg/ha)
    /// </summary>
    private double[] pot_c_decomp;

    /// <summary>
    /// Potential residue N decomposition (kg/ha)
    /// </summary>
    private double[] pot_n_decomp;

    /// <summary>
    /// Potential residue P decomposition (kg/ha)
    /// </summary>
    private double[] pot_p_decomp;

    /// <summary>
    /// Actual residue C decomposition (kg/ha)
    /// </summary>
    private double[][] dlt_c_decomp;

    /// <summary>
    /// Actual residue N decomposition (kg/ha)
    /// </summary>
    private double[][] dlt_n_decomp;

    #endregion

    #endregion internal variables

    #region Types and structures

    /// <summary>
    /// The parameters to compute a exponential type function (used for example for temperature factor)
    /// </summary>
    private struct BentStickData
    {
        // this is a bending stick type data

        /// <summary>
        /// Optimum temperature, when factor is equal to one
        /// </summary>
        public double[] xValueForOptimum;
        /// <summary>
        /// Value of factor when temperature is equal to zero celsius
        /// </summary>
        public double[] yValueAtZero;
        /// <summary>
        /// Exponent defining the curvature of the factors
        /// </summary>
        public double[] CurveExponent;
    }

    /// <summary>
    /// Lists of x and y values used to describe certain a 'broken stick' function (e.g. moisture factor)
    /// </summary>
    private struct BrokenStickData
    {
        /// <summary>
        /// The values in the x-axis
        /// </summary>
        public double[] xVals;
        /// <summary>
        /// The values in the y-axis
        /// </summary>
        public double[] yVals;
    }

    #endregion
}