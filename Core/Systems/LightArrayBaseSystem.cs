//using Radiance.Content.Items.BaseItems;

//namespace Radiance.Core.Systems
//{
//    public class LightArrayBaseSystem : ModSystem
//    {
//        public static Dictionary<Point16, LightArrayBase> lightArrays = new Dictionary<Point16, LightArrayBase>();
//        public override void SaveWorldData(TagCompound tag)
//        {
//            tag[nameof(lightArrays) + "Keys"] = lightArrays.Keys;
//            tag[nameof(lightArrays) +"Values"] = lightArrays.Values;
//        }
//        public override void LoadWorldData(TagCompound tag)
//        {
//            List<Point16> optionKeys = (List<Point16>)tag.GetList<Point16>(nameof(lightArrays) + "Keys");
//            if (optionKeys.Any())
//            {
//                List<LightArrayBase> optionValues = (List<LightArrayBase>)tag.GetList<LightArrayBase>(nameof(lightArrays) + "Values");
//                lightArrays = optionKeys.Zip(optionValues, (k, v) => new { Key = k, Value = v }).ToDictionary(x => x.Key, x => x.Value);
//            }
//        }
//        public override void ClearWorld()
//        {
//            lightArrays.Clear();
//        }
//    }
//    public class LightArrayBase : ILoadable, TagSerializable
//    {
//        public Point16 position;
//        public Item lightArrayItem;
//        public BaseLightArray lightArray => lightArrayItem.ModItem as BaseLightArray;
//        public Item[] items => lightArray.inventory;
//        //public string lightArrayType;
//        //public Dictionary<string, int> lightArrayOptions = new Dictionary<string, int>();
//        public LightArrayBase(Point16 position) 
//        { 
//            this.position = position;
//            //this.lightArrayType = lightArrayType;
//        }
//        public void Load(Mod mod)
//        {
//            //lightArrayOptions = ModContent.GetInstance<BaseLightArray>().optionsDictionary;
//        }

//        public void Unload() { }

//        #region TagCompound Stuff

//        public static readonly Func<TagCompound, LightArrayBase> DESERIALIZER = DeserializeData;

//        public TagCompound SerializeData()
//        {
//            return new TagCompound()
//            {
//                [nameof(position)] = position,
//                [nameof(lightArrayItem)] = lightArrayItem,
//                //[nameof(lightArrayType)] = lightArrayType,
//                //[nameof(lightArrayOptions) + "Keys"] = lightArrayOptions.Keys,
//                //[nameof(lightArrayOptions) + "Values"] = lightArrayOptions.Values
//            };
//        }

//        public static LightArrayBase DeserializeData(TagCompound tag)
//        {
//            LightArrayBase lightArrayBase = new(tag.Get<Point16>(nameof(position))) 
//            {
//                lightArrayItem = tag.Get<Item>(nameof(lightArrayItem)) 
//            };

//            //List<string> optionKeys = (List<string>)tag.GetList<string>(nameof(lightArrayOptions) + "Keys");
//            //if (optionKeys.Any())
//            //{
//            //    List<int> optionValues = (List<int>)tag.GetList<int>(nameof(lightArrayOptions) + "Values");
//            //    lightArrayBase.lightArrayOptions = optionKeys.Zip(optionValues, (k, v) => new { Key = k, Value = v }).ToDictionary(x => x.Key, x => x.Value);
//            //}
//            return lightArrayBase;
//        }
//        #endregion TagCompound Stuff
//    }
//}
