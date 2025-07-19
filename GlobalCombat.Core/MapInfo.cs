using System;
using System.Collections;
using System.Collections.Generic;

namespace GlobalCombat.Core
{
	public class AreaInfo
	{
        public int Number;
		public string Name;
		public string TechName;
		public int Region;
		public int X;
		public int Y;
		public int Width;
		public int Height;
		public int Link1;
		public int Link2;
		public int Link3;
		public int Link4;
		public int Link5;
		public int Link6;
        public List<AreaInfo> Inbounds = new List<AreaInfo>();

		public AreaInfo(int number, string name, string techName, int region, int x, int y, int width, int height, int link1, int link2, int link3, int link4, int link5, int link6)
		{
            Number = number;
			Name = name;
			TechName = techName;
			Region = region;
			X = x;
			Y = y;
			Width = width;
			Height = height;
			Link1 = link1;
			Link2 = link2;
			Link3 = link3;
			Link4 = link4;
			Link5 = link5;
			Link6 = link6;
		}

        public void Linkup(MapInfo map)
        {
            foreach (AreaInfo area in map.Areas)
                if (area.LinksTo(Number))
                    Inbounds.Add(area);
        }

        public bool LinksTo(int target)
        {
            return
                Link1 == target ||
                Link2 == target ||
                Link3 == target ||
                Link4 == target ||
                Link5 == target ||
                Link6 == target;
        }

		public bool LinksTo(AreaInfo areaInfo)
		{
			return LinksTo(areaInfo.Number);
		}
	}

	public class RegionInfo
	{
		public int Number;
		public string Name;
		public int NumAreas;
		public int ArmyBonus;

		public RegionInfo(int number, string AName, int ANumAreas, int AArmyBonus)
		{
			Number = number;
			Name = AName;
			NumAreas = ANumAreas;
			ArmyBonus = AArmyBonus;
		}
	}

	public class MapInfo
	{
		public string TechName;
		public string Name;
		public int NumAreas;
		public int NumRegions;
		public List<AreaInfo> Areas;
		public List<RegionInfo> Regions;
		public string AreaMap;

		public AreaInfo GetArea(int AAreaID)
		{
			return Areas[AAreaID - 1];
		}

		public RegionInfo GetRegion(int ARegionID)
		{
			return Regions[ARegionID - 1];
		}

		public void LoadMapInfo(string ATechName)
		{
			TechName = ATechName;

			if (TechName == "original")
			{
				Name = "World War I Era";
	
				NumAreas = 42;
				NumRegions = 6;
	
				Areas = new List<AreaInfo>(NumAreas);
				Areas.Add(new AreaInfo(1, "Alaska", "alaska", 1, 135, 117, 64, 79, 2, 3, 37, 0, 0, 0));
				Areas.Add(new AreaInfo(2, "Northern Territories", "northern", 1, 182, 75, 144, 100, 1, 3, 4, 5, 9, 0));
				Areas.Add(new AreaInfo(3, "Alberta", "alberta", 1, 177, 142, 47, 77, 1, 2, 4, 6, 0, 0));
				Areas.Add(new AreaInfo(4, "Ontario", "ontario", 1, 210, 152, 69, 67, 2, 3, 5, 6, 7, 0));
				Areas.Add(new AreaInfo(5, "Quebec", "quebec", 1, 256, 160, 90, 61, 2, 4, 6, 7, 9, 0));
				Areas.Add(new AreaInfo(6, "West United States", "west", 1, 203, 212, 59, 39, 3, 4, 5, 7, 8, 0));
				Areas.Add(new AreaInfo(7, "East United States", "east", 1, 239, 204, 108, 67, 4, 5, 6, 8, 0, 0));
				Areas.Add(new AreaInfo(8, "Mexico", "mexico", 1, 216, 247, 104, 60, 6, 7, 10, 0, 0, 0)); 
				Areas.Add(new AreaInfo(9, "Greenland", "greenland", 1, 343, 65, 80, 123, 2, 5, 20, 0, 0, 0)); 
				Areas.Add(new AreaInfo(10, "Columbia", "columbia", 2, 282, 296, 58, 36, 8, 11, 12, 0, 0, 0)); 
				Areas.Add(new AreaInfo(11, "Peru", "peru", 2, 275, 321, 51, 61, 10, 12, 13, 0, 0, 0)); 
				Areas.Add(new AreaInfo(12, "Brazil", "brazil", 2, 293, 324, 71, 75, 10, 11, 13, 14, 0, 0)); 
				Areas.Add(new AreaInfo(13, "Argentina", "argentina", 2, 290, 371, 39, 87, 11, 12, 0, 0, 0, 0)); 
				Areas.Add(new AreaInfo(14, "Algeria", "algeria", 3, 405, 264, 63, 73, 12, 15, 16, 22, 0, 0)); 
				Areas.Add(new AreaInfo(15, "Egypt", "egypt", 3, 459, 274, 65, 50, 14, 16, 17, 27, 25, 0)); 
				Areas.Add(new AreaInfo(16, "Congo", "congo", 3, 429, 307, 71, 54, 14, 15, 17, 18, 0, 0)); 
				Areas.Add(new AreaInfo(17, "Tanzania", "tanzania", 3, 474, 319, 67, 63, 15, 16, 18, 19, 0, 0)); 
				Areas.Add(new AreaInfo(18, "South Africa", "southAfrica", 3, 459, 352, 52, 67, 16, 17, 19, 0, 0, 0)); 
				Areas.Add(new AreaInfo(19, "Madagascar", "madagascar", 3, 520, 353, 27, 47, 17, 18, 0, 0, 0, 0)); 
				Areas.Add(new AreaInfo(20, "Iceland", "iceland", 4, 406, 149, 24, 28, 9, 21, 24, 0, 0, 0)); 
				Areas.Add(new AreaInfo(21, "United Kingdom", "united", 4, 410, 189, 31, 40, 20, 22, 23, 24, 0, 0)); 
				Areas.Add(new AreaInfo(22, "Western Europe", "france", 4, 419, 219, 35, 46, 14, 21, 23, 0, 0, 0)); 
				Areas.Add(new AreaInfo(23, "Austria", "italy", 4, 444, 201, 32, 63, 21, 22, 24, 25, 0, 0)); 
				Areas.Add(new AreaInfo(24, "Scandinavia", "scandinavia", 4, 447, 138, 87, 72, 20, 21, 23, 25, 26, 0)); 
				Areas.Add(new AreaInfo(25, "Eastern Europe", "greece", 4, 465, 201, 65, 62, 15, 23, 24, 26, 27, 28)); 
				Areas.Add(new AreaInfo(26, "Ukraine", "hungary", 4, 508, 127, 75, 103, 24, 25, 28, 30, 0, 0)); 
				Areas.Add(new AreaInfo(27, "Middle East", "turkey", 5, 491, 247, 69, 68, 15, 25, 28, 29, 0, 0)); 
				Areas.Add(new AreaInfo(28, "Iran", "iran", 5, 513, 179, 79, 94, 25, 26, 27, 29, 30, 0)); 
				Areas.Add(new AreaInfo(29, "India", "india", 5, 553, 254, 76, 74, 27, 28, 30, 33, 0, 0)); 
				Areas.Add(new AreaInfo(30, "Kazakhstan", "kazakhstan", 5, 560, 127, 79, 157, 26, 28, 29, 31, 32, 33)); 
				Areas.Add(new AreaInfo(31, "Ural", "ural", 5, 600, 83, 69, 148, 30, 32, 34, 35, 0, 0)); 
				Areas.Add(new AreaInfo(32, "China", "china", 5, 619, 209, 112, 75, 30, 31, 33, 34, 38, 0)); 
				Areas.Add(new AreaInfo(33, "Thailand", "thailand", 5, 616, 269, 67, 62, 29, 30, 32, 39, 0, 0)); 
				Areas.Add(new AreaInfo(34, "Mongolia", "mongolia", 5, 648, 178, 79, 51, 31, 32, 35, 36, 37, 38)); 
				Areas.Add(new AreaInfo(35, "Siberia", "siberia", 5, 643, 117, 54, 79, 31, 34, 36, 0, 0, 0)); 
				Areas.Add(new AreaInfo(36, "Chersky", "cherskiy", 5, 672, 117, 60, 72, 35, 34, 37, 0, 0, 0)); 
				Areas.Add(new AreaInfo(37, "Pevek", "pevek", 5, 720, 137, 63, 49, 34, 36, 1, 0, 0, 0)); 
				Areas.Add(new AreaInfo(38, "Japan", "japan", 5, 719, 199, 57, 56, 32, 34, 0, 0, 0, 0)); 
				Areas.Add(new AreaInfo(39, "Indonesia", "indonesia", 6, 624, 299, 88, 70, 40, 41, 42, 33, 0, 0)); 
				Areas.Add(new AreaInfo(40, "Outback", "albany", 6, 642, 376, 56, 54, 39, 41, 42, 0, 0, 0)); 
				Areas.Add(new AreaInfo(41, "Queensland", "sydney", 6, 691, 373, 89, 86, 39, 40, 42, 0, 0, 0)); 
				Areas.Add(new AreaInfo(42, "New Guinea", "newZealand", 6, 700, 355, 73, 30, 39, 40, 41, 0, 0, 0));
	
				Regions = new List<RegionInfo>(NumRegions);
				Regions.Add(new RegionInfo(1, "North America", 9, 6)); 
				Regions.Add(new RegionInfo(2, "South America", 4, 3)); 
				Regions.Add(new RegionInfo(3, "Africa", 6, 4)); 
				Regions.Add(new RegionInfo(4, "Europe", 7, 7)); 
				Regions.Add(new RegionInfo(5, "Asia", 12, 9)); 
				Regions.Add(new RegionInfo(6, "Australia", 4, 2));
	
				AreaMap = "";
			}
			else
			{
				Name = "Battle of the Elements";
	
				NumAreas = 38;
				NumRegions = 6;

                Areas = new List<AreaInfo>(NumAreas);
				Areas.Add(new AreaInfo(1, "Fire Corner", "cf", 1, 410, 123, 107, 107, 5, 10, 11, 12, 13, 14)); 
				Areas.Add(new AreaInfo(2, "Wind Corner", "ca", 1, 322, 211, 107, 107, 5, 26, 27, 28, 29, 30)); 
				Areas.Add(new AreaInfo(3, "Earth Corner", "ce", 1, 411, 300, 107, 107, 6, 34, 35, 36, 37, 38)); 
				Areas.Add(new AreaInfo(4, "Water Corner", "cw", 1, 499, 212, 107, 107, 6, 18, 19, 20, 21, 22)); 
				Areas.Add(new AreaInfo(5, "Smoke Bridge", "bs", 2, 374, 176, 91, 90, 6, 1, 2, 0, 0, 0)); 
				Areas.Add(new AreaInfo(6, "Mud Bridge", "bm", 2, 463, 264, 91, 90, 5, 3, 4, 0, 0, 0)); 
				Areas.Add(new AreaInfo(7, "Steam", "f1o", 3, 604, 142, 74, 71, 8, 17, 0, 0, 0, 0)); 
				Areas.Add(new AreaInfo(8, "Carbon Oxidization", "f2o", 3, 489, 70, 81, 81, 9, 11, 12, 9, 0, 0)); 
				Areas.Add(new AreaInfo(9, "Flame", "f3o", 3, 357, 70, 81, 81, 13, 14, 23, 8, 0, 0)); 
				Areas.Add(new AreaInfo(10, "Hydrogen", "ftow", 3, 464, 176, 71, 73, 1, 18, 0, 0, 0, 0)); 
				Areas.Add(new AreaInfo(11, "Burn", "f5i", 3, 489, 123, 55, 54, 1, 8, 12, 0, 0, 0)); 
				Areas.Add(new AreaInfo(12, "Torch", "f6i", 3, 462, 96, 56, 56, 1, 8, 11, 0, 0, 0)); 
				Areas.Add(new AreaInfo(13, "Blaze", "f7i", 3, 410, 96, 54, 55, 1, 9, 14, 0, 0, 0)); 
				Areas.Add(new AreaInfo(14, "Spark", "f8i", 3, 383, 122, 56, 56, 1, 9, 13, 0, 0, 0)); 
				Areas.Add(new AreaInfo(15, "Water 1", "w1o", 4, 606, 318, 71, 73, 16, 0, 0, 0, 0, 0)); 
				Areas.Add(new AreaInfo(16, "Water 2", "w2o", 4, 579, 291, 80, 81, 15, 19, 20, 17, 0, 0)); 
				Areas.Add(new AreaInfo(17, "Water 3", "w3o", 4, 578, 159, 81, 81, 7, 21, 22, 16, 0, 0)); 
				Areas.Add(new AreaInfo(18, "Water to Fire", "wtof", 4, 482, 194, 71, 73, 4, 10, 0, 0, 0, 0)); 
				Areas.Add(new AreaInfo(19, "Water 5", "w5i", 4, 552, 291, 55, 54, 4, 16, 20, 0, 0, 0)); 
				Areas.Add(new AreaInfo(20, "Water 6", "w6i", 4, 579, 265, 53, 53, 4, 16, 19, 0, 0, 0)); 
				Areas.Add(new AreaInfo(21, "Water 7", "w7i", 4, 577, 212, 56, 56, 4, 17, 22, 0, 0, 0)); 
				Areas.Add(new AreaInfo(22, "Water 8", "w8i", 4, 551, 186, 55, 54, 4, 17, 21, 0, 0, 0)); 
				Areas.Add(new AreaInfo(23, "Wind 1", "a1o", 5, 252, 141, 71, 73, 24, 0, 0, 0, 0, 0)); 
				Areas.Add(new AreaInfo(24, "Wind 2", "a2o", 5, 269, 159, 81, 81, 23, 27, 28, 25, 0, 0)); 
				Areas.Add(new AreaInfo(25, "Wind 3", "a3o", 5, 270, 290, 81, 80, 29, 30, 31, 24, 0, 0)); 
				Areas.Add(new AreaInfo(26, "Wind to Earth", "atoe", 5, 375, 264, 71, 73, 2, 34, 0, 0, 0, 0)); 
				Areas.Add(new AreaInfo(27, "Wind 5", "a5i", 5, 321, 185, 54, 55, 2, 24, 28, 0, 0, 0)); 
				Areas.Add(new AreaInfo(28, "Wind 6", "a6i", 5, 295, 210, 56, 56, 2, 24, 27, 0, 0, 0)); 
				Areas.Add(new AreaInfo(29, "Wind 7", "a7i", 5, 296, 265, 53, 53, 2, 25, 30, 0, 0, 0)); 
				Areas.Add(new AreaInfo(30, "Wind 8", "a8i", 5, 322, 290, 54, 55, 2, 25, 29, 0, 0, 0)); 
				Areas.Add(new AreaInfo(31, "Earth 1", "e1o", 6, 251, 317, 74, 71, 25, 32, 0, 0, 0, 0)); 
				Areas.Add(new AreaInfo(32, "Earth 2", "e2o", 6, 358, 380, 81, 80, 35, 36, 33, 0, 0, 0)); 
				Areas.Add(new AreaInfo(33, "Earth 3", "e3o", 6, 491, 379, 80, 81, 37, 38, 32, 15, 0, 0)); 
				Areas.Add(new AreaInfo(34, "Earth to Wind", "etoa", 6, 393, 282, 71, 73, 3, 26, 0, 0, 0, 0)); 
				Areas.Add(new AreaInfo(35, "Earth 5", "e5i", 6, 385, 354, 53, 53, 3, 32, 36, 0, 0, 0)); 
				Areas.Add(new AreaInfo(36, "Earth 6", "e6i", 6, 411, 379, 54, 55, 3, 32, 35, 0, 0, 0)); 
				Areas.Add(new AreaInfo(37, "Earth 7", "e7i", 6, 463, 379, 55, 54, 3, 33, 38, 0, 0, 0)); 
				Areas.Add(new AreaInfo(38, "Earth 8", "e8i", 6, 491, 353, 53, 53, 3, 33, 37, 0, 0, 0));

                Regions = new List<RegionInfo>(NumRegions);
				Regions.Add(new RegionInfo(1, "Four Corners", 4, 7)); 
				Regions.Add(new RegionInfo(2, "Twin Bridges", 2, 3)); 
				Regions.Add(new RegionInfo(3, "Fire", 8, 5)); 
				Regions.Add(new RegionInfo(4, "Water", 8, 4)); 
				Regions.Add(new RegionInfo(5, "Earth", 8, 5)); 
				Regions.Add(new RegionInfo(6, "Wind", 8, 4));
			}

            foreach (AreaInfo area in Areas)
                area.Linkup(this);
		}

		public static void LoadMaps()
		{
            if (Maps == null)
            {
                Maps = new Dictionary<MapName, MapInfo>(2);

                if (!Maps.ContainsKey(MapName.Original))
                {
                    MapInfo newMap = new MapInfo();
                    newMap.LoadMapInfo("original");
                    Maps.Add(MapName.Original, newMap);
                }

                if (!Maps.ContainsKey(MapName.Elements))
                {
                    MapInfo newMap = new MapInfo();
                    newMap.LoadMapInfo("elements");
                    Maps.Add(MapName.Elements, newMap);
                }
            }
		}

		public static Dictionary<MapName, MapInfo> Maps;

		public static MapInfo GetMap(MapName name)
		{
			LoadMaps();
            return Maps[name];
		}
	}
}