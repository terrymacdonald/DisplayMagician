using DisplayMagician.GameLibraries;
using Microsoft.WindowsAPICodePack.Win32Native.Consts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DisplayMagician.FovCalculator;
using static System.Net.WebRequestMethods;
using static WinFormAnimation.AnimationFunctions;

namespace DisplayMagician
{
    public struct GameFovDetail
    {
        public FovType FovType;
        public string GameName;
        public string GamePublisher;
        public string GameURL;
        public double Min;
        public double Max;
        public double Decimals;
        public double Factor;
        public double BaseSingle;
        public double BaseTriple;
        public double Increment;
        public double Step;
        public double ResultDegrees;
        public double ResultTimes;
        public double ResultRadians;
        public double ResultBaseSteps;
    }

    public enum ScreenAspectRatio : uint
    {
        Custom = 0, 
        SixteenByNine = 1,
        SixteenByTen = 2,
        TwentyOneByNine = 3,
        TwentyOneByTen = 4,
        ThirtyTwoByNine = 5,
        ThirtyTwoByTen = 6,
        FiveByFour = 7,
        FourByThree = 8,
    }

    public enum ScreenLayout : uint
    {
        SingleScreen = 0,
        TripleScreen = 1,
    }

    public enum ScreenMeasurementUnit : uint
    {
        CM = 0,
        MM = 1,
        Inch = 2,
    }

    public enum FovType : uint
    {
        HorizontalFOVDegrees = 0,
        HorizontalFOVRadians = 1,
        HorizontalFOVBaseSteps = 2,
        VerticalFOVDegrees = 3,
        VerticalFOVTimes = 4,
        TripleScreenAngle = 5,
    }


    public static class FovCalculator
    {

        private static List<GameFovDetail> _gameFovDetails = new List<GameFovDetail>() {
            new GameFovDetail(){
                FovType = FovType.HorizontalFOVDegrees,
                GameName = "Horizontal FOV in Degrees",
                GamePublisher = "",
                GameURL = "",
                Min = 0,
                Max = 180,
                Decimals = 1,
                Factor = 1,
                BaseSingle = 0,
                BaseTriple = 0,
                Increment = 0,
                Step = 0
            },
            new GameFovDetail(){
                FovType = FovType.HorizontalFOVDegrees,
                GameName = "Project CARS 1",
                GamePublisher = "Slightly Mad Studios",
                GameURL = "https://store.steampowered.com/app/234630/Project_CARS/",
                Min = 35,
                Max = 180,
                Decimals = 0,
                Factor = 1,
                BaseSingle = 0,
                BaseTriple = 0,
                Increment = 0,
                Step = 0
            },
            new GameFovDetail(){
                FovType = FovType.HorizontalFOVDegrees,
                GameName = "Project CARS 2",
                GamePublisher = "Slightly Mad Studios",
                GameURL = "https://store.steampowered.com/app/378860/Project_CARS_2/",
                Min = 35,
                Max = 180,
                Decimals = 0,
                Factor = 1,
                BaseSingle = 0,
                BaseTriple = 0,
                Increment = 0,
                Step = 0
            },
            new GameFovDetail(){
                FovType = FovType.HorizontalFOVDegrees,
                GameName = "Project CARS 3",
                GamePublisher = "Slightly Mad Studios",
                GameURL = "https://store.steampowered.com/app/958400/Project_CARS_3/",
                Min = 35,
                Max = 180,
                Decimals = 0,
                Factor = 1,
                BaseSingle = 0,
                BaseTriple = 0,
                Increment = 0,
                Step = 0
            },

            new GameFovDetail(){
                FovType = FovType.HorizontalFOVDegrees,
                GameName = "Automobilista 2",
                GamePublisher = "Reiza Studios",
                GameURL = "https://store.steampowered.com/app/1066890/Automobilista_2/",
                Min = 35,
                Max = 180,
                Decimals = 0,
                Factor = 1,
                BaseSingle = 0,
                BaseTriple = 0,
                Increment = 0,
                Step = 0
            },
            new GameFovDetail(){
                FovType = FovType.HorizontalFOVDegrees,
                GameName = "European Truck Simulator 2",
                GamePublisher = "SCS Software",
                GameURL = "https://store.steampowered.com/app/227300/Euro_Truck_Simulator_2/",
                Min = 35,
                Max = 180,
                Decimals = 0,
                Factor = 1,
                BaseSingle = 0,
                BaseTriple = 0,
                Increment = 0,
                Step = 0
            },
            new GameFovDetail(){
                FovType = FovType.HorizontalFOVDegrees,
                GameName = "American Truck Simulator",
                GamePublisher = "SCS Software",
                GameURL = "https://store.steampowered.com/app/270880/American_Truck_Simulator/",
                Min = 35,
                Max = 180,
                Decimals = 0,
                Factor = 1,
                BaseSingle = 0,
                BaseTriple = 0,
                Increment = 0,
                Step = 0
            },
            new GameFovDetail(){
                FovType = FovType.HorizontalFOVRadians,
                GameName = "Richard Burns Rally",
                GamePublisher = "SCi",
                GameURL = "https://rallysimfans.hu/rbr/download.php?download=rsfrbr",
                Min = 10,
                Max = 180,
                Decimals = 6,
                Factor = 1,
                BaseSingle = 0,
                BaseTriple = 0,
                Increment = 0,
                Step = 0
            },
            new GameFovDetail(){
                FovType = FovType.HorizontalFOVBaseSteps,
                GameName = "F1 2016-2018 (In-car Camera)", // https://www.reddit.com/r/F1Game/comments/7x0of9/codemasters_f1_20162017_fov_slider/
                GamePublisher = "Codemasters",
                GameURL = "",
                Min = -1,
                Max = +1,
                Decimals = 2,
                Factor = 1,
                BaseSingle = 77,
                BaseTriple = 77,
                Increment = 2,
                Step = 0.05 //slider step
            },
            new GameFovDetail(){
                FovType = FovType.HorizontalFOVBaseSteps,
                GameName = "F1 2016-2018 (Nose Camera & T-Camera)", // https://www.reddit.com/r/F1Game/comments/7x0of9/codemasters_f1_20162017_fov_slider/
                GamePublisher = "Codemasters",
                GameURL = "",
                Min = -1,
                Max = +1,
                Decimals = 2,
                Factor = 1,
                BaseSingle = 82,
                BaseTriple = 82,
                Increment = 2,
                Step = 0.05 //slider step
            },
            new GameFovDetail(){
                FovType = FovType.HorizontalFOVBaseSteps,
                GameName = "F1 2016-2018 (T-Camer Offset)", // https://www.reddit.com/r/F1Game/comments/7x0of9/codemasters_f1_20162017_fov_slider/
                GamePublisher = "Codemasters",
                GameURL = "",
                Min = -1,
                Max = +1,
                Decimals = 2,
                Factor = 1,
                BaseSingle = 85,
                BaseTriple = 85,
                Increment = 2,
                Step = 0.05 //slider step
            },
            new GameFovDetail(){
                FovType = FovType.HorizontalFOVBaseSteps,
                GameName = "F1 2019-2020 (In-car Camera)", //https://forums.codemasters.com/topic/46401-f1-2019-fov-values/
                GamePublisher = "Codemasters",
                GameURL = "",
                Min = -2,
                Max = +2,
                Decimals = 2,
                Factor = 2,
                BaseSingle = 77,
                BaseTriple = 77,
                Increment = 2,
                Step = 0.05 //slider step
            },
            new GameFovDetail(){
                FovType = FovType.HorizontalFOVBaseSteps,
                GameName = "F1 2021 (In-car Camera)",
                GamePublisher = "Codemasters",
                GameURL = "",
                Min = -20,
                Max = +20,
                Decimals = 2,
                Factor = 20,
                BaseSingle = 77,
                BaseTriple = 77,
                Increment = 2,
                Step = 0.05 //slider step
            },
            new GameFovDetail(){
                FovType = FovType.VerticalFOVDegrees,
                GameName = "Vertical FOV in Degrees",
                GamePublisher = "",
                GameURL = "",
                Min = 0,
                Max = 180,
                Decimals = 1,
                Factor = 1,
                BaseSingle = 0,
                BaseTriple = 0,
                Increment = 0,
                Step = 0
            },
            new GameFovDetail(){
                FovType = FovType.VerticalFOVDegrees,
                GameName = "Assetto Corsa",
                GamePublisher = "Kunos Simulazioni",
                GameURL = "https://store.steampowered.com/app/244210/Assetto_Corsa/",
                Min = 10,
                Max = 120,
                Decimals = 1,
                Factor = 1,
                BaseSingle = 0,
                BaseTriple = 0,
                Increment = 0,
                Step = 0
            },
            new GameFovDetail(){
                FovType = FovType.VerticalFOVDegrees,
                GameName = "Assetto Corsa Competizione",
                GamePublisher = "Kunos Simulazioni",
                GameURL = "https://store.steampowered.com/app/805550/Assetto_Corsa_Competizione/",
                Min = 10,
                Max = 120,
                Decimals = 1,
                Factor = 1,
                BaseSingle = 0,
                BaseTriple = 0,
                Increment = 0,
                Step = 0
            },
            new GameFovDetail(){
                FovType = FovType.VerticalFOVDegrees,
                GameName = "rFactor 1",
                GamePublisher = "Image Space Incorporated",
                GameURL = "https://store.steampowered.com/app/339790/rFactor/",
                Min = 10,
                Max = 100,
                Decimals = 0,
                Factor = 1,
                BaseSingle = 0,
                BaseTriple = 0,
                Increment = 0,
                Step = 0
            },
            new GameFovDetail(){
                FovType = FovType.VerticalFOVDegrees,
                GameName = "rFactor 2",
                GamePublisher = "Studio 397",
                GameURL = "https://store.steampowered.com/app/365960/rFactor_2/",
                Min = 10,
                Max = 100,
                Decimals = 0,
                Factor = 1,
                BaseSingle = 0,
                BaseTriple = 0,
                Increment = 0,
                Step = 0
            },
            new GameFovDetail(){
                FovType = FovType.VerticalFOVDegrees,
                GameName = "Game Stock Car",
                GamePublisher = "Reiza Studios",
                GameURL = "https://store.steampowered.com/app/365960/rFactor_2/",
                Min = 10,
                Max = 100,
                Decimals = 0,
                Factor = 1,
                BaseSingle = 0,
                BaseTriple = 0,
                Increment = 0,
                Step = 0
            },
            new GameFovDetail(){
                FovType = FovType.VerticalFOVDegrees,
                GameName = "Formula Truck 2013",
                GamePublisher = "Reiza Studios",
                GameURL = "https://store.steampowered.com/app/273750/Formula_Truck_2013/",
                Min = 10,
                Max = 100,
                Decimals = 0,
                Factor = 1,
                BaseSingle = 0,
                BaseTriple = 0,
                Increment = 0,
                Step = 0
            },
            new GameFovDetail(){
                FovType = FovType.VerticalFOVDegrees,
                GameName = "Automobilista",
                GamePublisher = "Reiza Studios",
                GameURL = "https://store.steampowered.com/app/431600/Automobilista/",
                Min = 10,
                Max = 100,
                Decimals = 0,
                Factor = 1,
                BaseSingle = 0,
                BaseTriple = 0,
                Increment = 0,
                Step = 0
            },
            new GameFovDetail(){
                FovType = FovType.VerticalFOVDegrees,
                GameName = "DiRT Rally",
                GamePublisher = "Codemasters",
                GameURL = "https://store.steampowered.com/app/310560/DiRT_Rally/",
                Min = 10,
                Max = 115,
                Decimals = 0,
                Factor = 2,
                BaseSingle = 0,
                BaseTriple = 0,
                Increment = 0,
                Step = 0
            },
            new GameFovDetail(){
                FovType = FovType.VerticalFOVDegrees,
                GameName = "DiRT Rally 2.0",
                GamePublisher = "Codemasters",
                GameURL = "https://store.steampowered.com/app/690790/DiRT_Rally_20/",
                Min = 10,
                Max = 115,
                Decimals = 0,
                Factor = 2,
                BaseSingle = 0,
                BaseTriple = 0,
                Increment = 0,
                Step = 0
            },
            new GameFovDetail(){
                FovType = FovType.VerticalFOVDegrees,
                GameName = "GRID",
                GamePublisher = "Codemasters",
                GameURL = "https://store.steampowered.com/app/703860/GRID/",
                Min = 10,
                Max = 115,
                Decimals = 0,
                Factor = 2,
                BaseSingle = 0,
                BaseTriple = 0,
                Increment = 0,
                Step = 0
            },
            new GameFovDetail(){
                FovType = FovType.VerticalFOVTimes,
                GameName = "RaceRoom Racing Experience",
                GamePublisher = "KW Studios",
                GameURL = "https://store.steampowered.com/app/211500/RaceRoom_Racing_Experience/",
                Min = 0.5,
                Max = 1.5,
                Decimals = 1,
                Factor = 1,
                BaseSingle = 58,
                BaseTriple = 40,
                Increment = 0,
                Step = 0
            },
            new GameFovDetail(){
                FovType = FovType.VerticalFOVTimes,
                GameName = "GTR 2 FIA GT Racing Game",
                GamePublisher = "SimBin Studios",
                GameURL = "https://store.steampowered.com/app/8790/GTR_2_FIA_GT_Racing_Game/",
                Min = 0.5,
                Max = 1.5,
                Decimals = 1,
                Factor = 1,
                BaseSingle = 58,
                BaseTriple = 58,
                Increment = 0,
                Step = 0
            },
            new GameFovDetail(){
                FovType = FovType.VerticalFOVTimes,
                GameName = "RACE 07",
                GamePublisher = "SimBin Studios",
                GameURL = "https://store.steampowered.com/app/8600/RACE_07/",
                Min = 0.4,
                Max = 1.5,
                Decimals = 1,
                Factor = 1,
                BaseSingle = 58,
                BaseTriple = 58,
                Increment = 0,
                Step = 0
            },
            /*new GameFovDetail(){
                FovType = FovType.HorizontalFOVDegrees,
                GameName = "BeamNG.drive",
                GamePublisher = "BeamNG",
                GameURL = "https://store.steampowered.com/app/284160/BeamNGdrive/",
                Min = 0,
                Max = 0,
                Decimals = 0,
                Factor = 1,
                BaseSingle = 0,
                BaseTriple = 0,
                Increment = 0,
                Step = 0
            },*/
        }.OrderBy(tr => tr.GameName).ToList();


        public static ScreenLayout ScreenLayout { get; set; }

        public static ScreenAspectRatio ScreenRatio { get; set; }

        public static double ScreenRatioX { get; set; }

        public static double ScreenRatioY { get; set; }

        public static double ScreenSize { get; set; }

        public static ScreenMeasurementUnit ScreenSizeUnit { get; set; }

        public static double DistanceToScreen { get; set; }

        public static ScreenMeasurementUnit DistanceToScreenUnit { get; set; }

        public static double BezelSize { get; set; }

        public static ScreenMeasurementUnit BezelSizeUnit { get; set; }

        public static bool CalculateFOV(ScreenLayout screenLayout, ScreenAspectRatio screenRatio, double screenSize, ScreenMeasurementUnit screenSizeUnit, double distanceToScreen, ScreenMeasurementUnit distanceToScreenUnit, double bezelSize, ScreenMeasurementUnit bezelSizeUnit)
        {
            ScreenLayout = screenLayout;
            ScreenRatio = screenRatio;
            ScreenSize = screenSize;
            ScreenSizeUnit = screenSizeUnit;
            DistanceToScreen = distanceToScreen;
            DistanceToScreenUnit = distanceToScreenUnit;
            BezelSize = bezelSize;
            BezelSizeUnit = bezelSizeUnit;

            return CalculateFOV();
        }


        public static bool CalculateFOV()
        {
            // This will calculate the Field of View for each game, and store the answer in with the game
            // This will allow the result to be shown to the user via a form, yet have all the calculations performed here.

            // Convert ScreenSize to cm
            double screenSizeInCm = 0;
            if (ScreenSizeUnit == ScreenMeasurementUnit.Inch)
            {
                screenSizeInCm = ScreenSize * 2.54;
            }
            else if (ScreenSizeUnit == ScreenMeasurementUnit.MM)
            {
                screenSizeInCm = ScreenSize / 10;
            }
            else if (ScreenSizeUnit == ScreenMeasurementUnit.CM)
            {
                screenSizeInCm = ScreenSize;
            }
            else
            {
                // Unit supplied is not one we know about!
                return false;
            }

            // Convert distanceToScreen to cm
            double distanceToScreenInCm = 0;
            if (DistanceToScreenUnit == ScreenMeasurementUnit.Inch)
            {
                distanceToScreenInCm = DistanceToScreen * 2.54;
            }
            else if (DistanceToScreenUnit == ScreenMeasurementUnit.MM)
            {
                distanceToScreenInCm = DistanceToScreen / 10;
            }
            else if (DistanceToScreenUnit == ScreenMeasurementUnit.CM)
            {
                distanceToScreenInCm = DistanceToScreen;
            }
            else
            {
                // Unit supplied is not one we know about!
                return false;
            }


            double bezelSizeInCm = 0;
            if (ScreenLayout == ScreenLayout.TripleScreen)
            {
                // Convert bezelSize to cm
                if (BezelSizeUnit == ScreenMeasurementUnit.Inch)
                {
                    bezelSizeInCm = BezelSize * 2.54;
                }
                else if (BezelSizeUnit == ScreenMeasurementUnit.MM)
                {
                    bezelSizeInCm = BezelSize / 10;
                }
                else if (BezelSizeUnit == ScreenMeasurementUnit.CM)
                {
                    bezelSizeInCm = BezelSize;
                }
                else
                {
                    // Unit supplied is not one we know about!
                    return false;
                }
            }        

            // Check sensible minimums and maximums
            // Check that screen size is between 48cm and 508cm diagonally (19 inch to 200 inch screen sizes)
            if (screenSizeInCm < 48)
            {
                // Screen size is too small
                return false;
            }
            else if (screenSizeInCm > 508)
            {
                // Screen size is too big
                return false;
            }

            // Check that distance to screen is between 5cm and 10m
            if (distanceToScreenInCm < 5)
            {
                // Distance to screen is too small
                return false;
            }
            else if (distanceToScreenInCm > 10000)
            {
                // Distance to screen is too big
                return false;
            }

            // Check that bezel size is between 0 and 10cm
            if (bezelSizeInCm < 0)
            {
                // Bezel size is too small
                return false;
            }
            else if (bezelSizeInCm > 10)
            {
                // Bezel size is too big
                return false;
            }

            // If we get here we can start doing the calculation! Yay!
            double screenRatioX = 21;
            double screenRatioY = 9;
            if (ScreenRatio == ScreenAspectRatio.SixteenByNine)
            {
                screenRatioX = 16;
                screenRatioY = 9;
            }
            else if (ScreenRatio == ScreenAspectRatio.SixteenByTen)
            {
                screenRatioX = 16;
                screenRatioY = 10;
            }
            else if (ScreenRatio == ScreenAspectRatio.TwentyOneByNine)
            {
                screenRatioX = 21;
                screenRatioY = 9;
            }
            else if (ScreenRatio == ScreenAspectRatio.TwentyOneByTen)
            {
                screenRatioX = 21;
                screenRatioY = 10;
            }
            else if (ScreenRatio == ScreenAspectRatio.ThirtyTwoByNine)
            {
                screenRatioX = 32;
                screenRatioY = 9;
            }
            else if (ScreenRatio == ScreenAspectRatio.ThirtyTwoByTen)
            {
                screenRatioX = 32;
                screenRatioY = 10;
            }
            else if (ScreenRatio == ScreenAspectRatio.FiveByFour)
            {
                screenRatioX = 5;
                screenRatioY = 4;
            }
            else if (ScreenRatio == ScreenAspectRatio.FourByThree)
            {
                screenRatioX = 4;
                screenRatioY = 3;
            }
            else if (ScreenRatio == ScreenAspectRatio.Custom)
            {
                if (ScreenRatioX > 0)
                {
                    screenRatioX = ScreenRatioX;
                }                
                else
                {
                    return false;
                }
                if (ScreenRatioY > 0)
                {
                    screenRatioY = ScreenRatioY;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            int screenCount = 3;
            if (ScreenLayout == ScreenLayout.TripleScreen)
            {
                screenCount = 3;
            }
            else
            {
                screenCount = 1;
            }

            // Calculate the constants we need
            double screenRatioDouble = screenRatioY / screenRatioX;
            double bezelThickness = 2 * bezelSizeInCm;
            double height = Math.Sin(Math.Atan(screenRatioDouble)) * screenSizeInCm;
            double width = Math.Cos(Math.Atan(screenRatioDouble)) * screenSizeInCm + (screenCount > 1 ? bezelThickness : 0);
            double hAngle = calcAngle(width, distanceToScreenInCm);
            double vAngle = calcAngle(height, distanceToScreenInCm);
            double arcConstant = (180 / Math.PI);


            for (int i = 0; i < _gameFovDetails.Count; i++)
            {
                GameFovDetail game = _gameFovDetails[i];
                if (game.FovType == FovType.HorizontalFOVDegrees)
                {
                    double value = (arcConstant * (hAngle * screenCount)) * game.Factor;
                    if (value > game.Max)
                        value = game.Max;
                    else if (value < game.Min)
                        value = game.Min;

                    double base10 = Math.Pow(10, game.Decimals);
                    game.ResultDegrees = Math.Round((value * base10) / base10, (int)game.Decimals);

                }
                else if (game.FovType == FovType.HorizontalFOVRadians)
{
                    double value = (arcConstant * calcAngle(width / screenRatioX * screenRatioY / 3 * 4, distanceToScreenInCm)) * game.Factor;
                    if (value > game.Max)
                        value = game.Max;
                    else if (value < game.Min)
                        value = game.Min;

                    value *= (Math.PI / 180);

                    double base10 = Math.Pow(10, game.Decimals);
                    game.ResultRadians = Math.Round((value * base10) / base10, (int)game.Decimals);
                }
                else if (game.FovType == FovType.HorizontalFOVBaseSteps)
                {
                    double value = (arcConstant * (hAngle * screenCount));
                    value = Math.Round((value - game.BaseTriple) / game.Increment) * game.Step;
                    value *= game.Factor;

                    if (value > game.Max)
                        value = game.Max;
                    else if (value < game.Min)
                        value = game.Min;

                    double base10 = Math.Pow(10, game.Decimals);
                    game.ResultBaseSteps = Math.Round((value * base10) / base10, (int)game.Decimals);
                }
                else if (game.FovType == FovType.VerticalFOVDegrees)
                {
                    double value = (arcConstant * vAngle) * game.Factor;
                    if (value > game.Max)
                        value = game.Max;
                    else if (value < game.Min)
                        value = game.Min;

                    double base10 = Math.Pow(10, game.Decimals);
                    game.ResultDegrees = Math.Round((value * base10) / base10, (int)game.Decimals);
                }
                else if (game.FovType == FovType.VerticalFOVTimes)
                {
                    double value = (arcConstant * vAngle) * game.Factor;
                    value /= (screenCount == 1 ? game.BaseSingle : game.BaseTriple);

                    if (value > game.Max)
                        value = game.Max;
                    else if (value < game.Min)
                        value = game.Min;

                    double base10 = Math.Pow(10, game.Decimals);
                    game.ResultTimes = Math.Round((value * base10) / base10, (int)game.Decimals);
                }
                else if (game.FovType == FovType.TripleScreenAngle)
                {
                    double value = (arcConstant * hAngle) * game.Factor;
                    if (value > game.Max)
                        value = game.Max;
                    else if (value < game.Min)
                        value = game.Min;

                    double base10 = Math.Pow(10, game.Decimals);
                    game.ResultDegrees = Math.Round((value * base10) / base10, (int)game.Decimals);
                }
                else
                {
                    // Unknown type of FOV requested
                    return false;
                }
            }

            return true;
        }

        private static double calcAngle(double baseInCm, double distanceToMonitorInCm)
        {
            return (Math.Atan(baseInCm / 2 / distanceToMonitorInCm) * 2);
            // return angle * (180 / Math.PI);
        }
    }
}
    