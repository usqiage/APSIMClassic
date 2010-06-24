using System;
using System.Collections.Generic;
using System.Text;
using ModelFramework;
using VBMet;
using CSGeneral;

[Model]
public class Plant : Instance
   {
   // ------------------------------- Private data ----------------------------------
   private NamedList<Organ> _Organs = new NamedList<Organ>();
   [Output("Crop_Type")][Param] private string CropType = "";
   [Output] private double WaterSupplyDemandRatio = 0;

   // ---------------------------- Events we will send ------------------------------
   //[Event] public event OnNewCrop NewCropEvent;
   [Event] public event ApsimTypeDelegate NewCrop;
   [Event] public event ApsimTypeDelegate Sowing;

   public NamedList<Organ> Organs
      {
      // Public property to return our organs to caller. Used primarily for unit testing.
      get { return _Organs; }
      }
   [Input] public int day;
   [Input] public int year;

   /// <summary>
   /// Return today as a DateTime class.
   /// </summary>
   public DateTime Today
      {
      get
         {
         DateTime x = new DateTime(year, 1, 1);
         x = x.AddDays(day - 1);
         return x;
         }
      }   
   
   // ---------------------------- Events we subscribe to ---------------------------
   [EventHandler] public void OnSow (SowType Sow)
      {
      // Go through all our children and find all organs.
      foreach (Instance Child in Children)
         {
         if (Child is Organ)
            Organs.Add((Organ)Child);
         }

      PublishNewCropEvent();
      PublishSowingEvent();
      }


   [Output]
   public string PlantStatus
      {
      get
      { return "in"; }
      }
   [EventHandler] public void OnProcess()
      {
      DoPhenology();
      DoPotentialGrowth();

      DoWater();
      DoArbitrator();
      DoActualGrowth();
      }

   private void DoArbitrator()
      {
      if (IndexOf("Arbitrator") != -1)
         {
         Arbitrator A = (Arbitrator)Children["Arbitrator"];
         A.DoDM(Organs);
         A.DoN(Organs);
         }
      // DPH: Commentted out the exception throw as Slurp doesn't have an arbitrator.
      //else
      //   throw new Exception("Cannont find Arbitrator component in model description.");
      }
   public void DoPotentialGrowth()
      {
      foreach (Organ o in Organs)
         o.DoPotentialGrowth();
      }
   public void DoActualGrowth()
      {
      foreach (Organ o in Organs)
         o.DoActualGrowth();
      }
   private void DoPhenology() 
      {
      if (IndexOf("Phenology") != -1)
         {
         Phenology P = (Phenology)Children["Phenology"];
         P.DoTimeStep();
         }
      }
   private void DoWater()
      {
      double Supply = 0;
      double Demand = 0;
      foreach (Organ o in Organs)
         {
         Supply += o.WaterSupply;
         Demand += o.WaterDemand;
         }

      if (Demand > 0)
         WaterSupplyDemandRatio = Supply / Demand;
      else
         WaterSupplyDemandRatio = 0;

      double fraction = 1;
      if (Demand>0)
         fraction =      Math.Min(1.0,Supply / Demand);

      foreach (Organ o in Organs)
         {
         if (o.WaterDemand > 0)
            o.WaterAllocation = fraction * o.WaterDemand;
         }

      double FractionUsed = 0;
      if (Supply > 0)
         FractionUsed = Math.Min(1.0, Demand / Supply);

      foreach (Organ o in Organs)
         o.DoWaterUptake(FractionUsed*Supply);
      }
   [Output][Units("mm")]
   private double WaterDemand
      {
      get
         {
         double Demand = 0;
         foreach (Organ o in Organs)
            Demand += o.WaterDemand;
         return Demand;
         }
      }
   [Output]
   [Units("(g/m^2)")]
   public double AboveGroundDM
      {
      get
         {
         double Total = 0;
         foreach (Organ o in Organs)
            if (o is AboveGround)
               Total += o.Live.Wt + o.Dead.Wt;
         return Total;
         }
      }
   [Output]
   public double AboveGroundN
   {
       get
       {
           double Total = 0;
           foreach (Organ o in Organs)
               if (o is AboveGround)
                   Total += o.Live.N + o.Dead.N;
           return Total;
       }
   }
    [Output]
   public double BelowGroundDM
      {
      get
         {
         double Total = 0;
         foreach (Organ o in Organs)
            if (o is BelowGround)
               Total += o.Live.Wt + o.Dead.Wt;
         return Total;
         }
      }
    [Output]
    public double BelowGroundN
    {
        get
        {
            double Total = 0;
            foreach (Organ o in Organs)
                if (o is BelowGround)
                    Total += o.Live.N + o.Dead.N;
            return Total;
        }
    }

    [Output]
   public double TotalDM
      {
      get
         {
         double Total = 0;
         foreach (Organ o in Organs)
            Total += o.Live.Wt + o.Dead.Wt;
         return Total;
         }
      }
   [Output] public double TotalN
       {
       get
          {
          double Total = 0;
          foreach (Organ o in Organs)
             Total += o.Live.N + o.Dead.N;
          return Total;
          }
       }
   [Output]
   public double ReserveDM
      {
      get
         {
         double Total = 0;
         foreach (Organ o in Organs)
            if (o is ReserveOrgan)
               Total += o.Live.Wt + o.Dead.Wt;
         return Total;
         }
      }
   [Output]
   public double ReserveN
   {
       get
       {
           double Total = 0;
           foreach (Organ o in Organs)
               if (o is ReserveOrgan)
                   Total += o.Live.N + o.Dead.N;
           return Total;
       }
   }
   public Phenology Phenology
      {
      get
         {
         return (Phenology)Children["Phenology"];
         }
      }

      [Event] public event NullTypeDelegate Cutting;
      [EventHandler]
      private void OnCut()
         {
         Cutting.Invoke();
         }
   #region Events we're publishing
   private void PublishNewCropEvent()
      {
      // Send out New Crop Event to tell other modules who I am and what I am
      if (NewCrop != null)
         {
         NewCropType Crop = new NewCropType();
         Crop.crop_type = CropType;
         Crop.sender = Name;
         NewCrop.Invoke(Crop);
         }
      }
   private void PublishSowingEvent()
      {
      // Send out New Crop Event to tell other modules who I am and what I am
      if (Sowing != null)
         {
         NullType S = new NullType();
         Sowing.Invoke(S);
         }
      }
   #endregion



   }
   
