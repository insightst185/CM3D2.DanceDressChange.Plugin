using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityInjector;
using UnityInjector.Attributes;

namespace DanceDressChange.Plugin
{
    [PluginFilter("CM3D2x64"), PluginFilter("CM3D2x86"), PluginFilter("CM3D2VRx64"), PluginFilter("CM3D2OHx64"), PluginFilter("CM3D2OHx86"), PluginFilter("CM3D2OHVRx64"),
     PluginName("DanceDressChange"), PluginVersion("0.0.0.2")]

    public class DanceDressChange : PluginBase
    {
        private XmlManager xmlManager;
        private Maid maid;
        private int iSceneLevel;

        private enum TargetLevel
        {
            //ダンス1:ドキドキ☆Fallin' Love
            SceneDance_DDFL = 4,

            // ダンス2:entrance to you
            SceneDance_ETYL = 20,

            // ダンス3:scarlet leap
            SceneDance_SCLP = 22,

            // ダンス4:stellar my tears
            SceneDance_STMT = 26,

            // ダンス5:rhythmix to you
           SceneDance_RYFU = 28,

            // ダンス6:happy!happy!スキャンダル
            SceneDance_HAPY = 30,

            // ダンス6:happy!happy!スキャンダル豪華版
            SceneDance_HAP2 = 31,

            // ダンス7:
            SceneDance_KANO = 32
        }

        private string[] tagElements = 
        {
            "wear",
            "mizugi",
            "onepiece",
            "skirt",
            "bra",
            "panz"
        };

        // presetリスト 4人分でいいか ダンス用だからね
        private const int MAX_LISTED_MAID = 4;
        private int[] presetPos = new int[MAX_LISTED_MAID];

        private void SetPreset(Maid maid, string fileName)
        {
            //カスタムメイド3D したらば談話室 「カスタムメイド3D2」改造スレッド その8 >>11さんのコードからの引用
            var preset = GameMain.Instance.CharacterMgr.PresetLoad(Path.Combine(Path.GetFullPath(".\\") + "Preset", fileName));
            GameMain.Instance.CharacterMgr.PresetSet(maid, preset);
        }

        public void Awake()
        {
            UnityEngine.Object.DontDestroyOnLoad(this);
            xmlManager = new XmlManager(tagElements);
        }

        public void OnDestroy()
        {

        }

        public void OnLevelWasLoaded(int level)
        {
            iSceneLevel = level;

            Initialization();
        }

        //初期化処理
        private void Initialization()
        {
            for(int maidNo = 0; maidNo < MAX_LISTED_MAID; maidNo++){
                presetPos[maidNo] = 0;
            }
        }

        public void Start()
        {
        }

        public void Update()
        {

            if (Enum.IsDefined(typeof(TargetLevel),iSceneLevel)){
                for(int maidNo = 0; maidNo < MAX_LISTED_MAID; maidNo++){
                    if(Input.GetKeyDown(xmlManager.GetKey(maidNo))){
                        string presetFileName = xmlManager.GetPresetFileName(maidNo,presetPos[maidNo]);
                        if(presetFileName != null){
                            maid = GameMain.Instance.CharacterMgr.GetMaid(maidNo);
                            if (maid != null) {
                                SetPreset(maid,presetFileName);
                                presetPos[maidNo]++;
                            }
                        }
                    }
                }
                for(int maidNo = 0; maidNo < MAX_LISTED_MAID; maidNo++){
                    int i = 0;
                    foreach (string tagElement in tagElements){
                        if(Input.GetKeyDown(xmlManager.GetPororiKey(maidNo)) &&
                           Input.GetKey(xmlManager.GetTagKey(i)))
                        {
                        //   https://gist.github.com/neguse11/1951c3625ee7aa153a2a ,MaidVoicePitchPlugin.csを参考にpororiを呼び出してみる実験
                            this.gameObject.SendMessage("changePropPororiSetTag",tagElement);
                            this.gameObject.SendMessage("changePropPororiSetMaidNo",maidNo.ToString());
                            this.gameObject.SendMessage("checkPropPororiExec");
                            this.gameObject.SendMessage("changePropPororiExec");
                        }
                        i++;
                    }
                }
            }

        }

        //------------------------------------------------------xml--------------------------------------------------------------------
        private class XmlManager
        {
            private string xmlFileName = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\Config\DanceDressChange.xml";
            private XmlDocument xmldoc = new XmlDocument();
            List<string>[] listPreset = new List<string>[MAX_LISTED_MAID];
            KeyCode[] keyAttr = new KeyCode[MAX_LISTED_MAID];
            KeyCode[] keyAttrPorori = new KeyCode[MAX_LISTED_MAID];
            KeyCode[] keyAttrTag = new KeyCode[10]; // tagElementsの要素数とってこれないかな
            string[] tagElements;
            
            public XmlManager(string[] tagElements)
            {
                this.tagElements = tagElements;
                for(int i=0; i < MAX_LISTED_MAID; i++){
                    listPreset[i] = new List<string>();
                }
                
                try{
                    InitXml();
                }
                catch(Exception e){
                    Debug.LogError("DanceDressChange.Plugin:" + e.Source + e.Message + e.StackTrace);
                }
            }


            private void InitXml()
            {
                xmldoc.Load(xmlFileName);
                XmlNode keyConfig = xmldoc.GetElementsByTagName("KeyConfig")[0];
                for(int i = 0; i < MAX_LISTED_MAID; i++){
                    string KeyCode = ((XmlElement)keyConfig).GetAttribute("maid"+i);
                    foreach (string keyName in Enum.GetNames(typeof(KeyCode)))
                    {
                        if (KeyCode.Equals(keyName))
                        {
                            keyAttr[i] = (KeyCode)Enum.Parse(typeof(KeyCode), KeyCode);
                            break;
                        }
                    }
                }
                keyConfig = xmldoc.GetElementsByTagName("pororiConfig")[0];
                for(int i = 0; i < MAX_LISTED_MAID; i++){
                    string KeyCode = ((XmlElement)keyConfig).GetAttribute("maid"+i);
                    foreach (string keyName in Enum.GetNames(typeof(KeyCode)))
                    {
                        if (KeyCode.Equals(keyName))
                        {
                            keyAttrPorori[i] = (KeyCode)Enum.Parse(typeof(KeyCode), KeyCode);
                            break;
                        }
                    }
                }
                {
                    int i = 0;
                    foreach (string tagElement in tagElements)
                    {
                        string KeyCode = ((XmlElement)keyConfig).GetAttribute(tagElement);
                        foreach (string keyName in Enum.GetNames(typeof(KeyCode)))
                        {
                            if (KeyCode.Equals(keyName))
                            {
                                keyAttrTag[i] = (KeyCode)Enum.Parse(typeof(KeyCode), KeyCode);
                                break;
                            }
                        }
                        i++;
                    }
                }
                XmlNodeList presetList = xmldoc.GetElementsByTagName("PresetList");
                foreach (XmlNode presetFile in presetList)
                {
                    int maidNo = Int32.Parse(((XmlElement)presetFile).GetAttribute("maidNo"));
                    XmlNodeList fileNames = ((XmlElement)presetFile).GetElementsByTagName("File");
                    foreach (XmlNode fileName in fileNames){
                        listPreset[maidNo].Add(((XmlElement)fileName).GetAttribute("Name"));
                    }
                }
            }

            public KeyCode GetKey(int maidNo)
            {
                return keyAttr[maidNo];
            }

            public KeyCode GetPororiKey(int maidNo)
            {
                return keyAttrPorori[maidNo];
            }

            public KeyCode GetTagKey(int tagNo)
            {
                return keyAttrTag[tagNo];
            }
            
            public string GetPresetFileName(int no,int pos){
                string[] presetFileNames = listPreset[no].ToArray();
                if (presetFileNames[pos].Length == 0)
                {
                    return null;
                }
                else{
                    return presetFileNames[pos];
                }
            }

        }
    }
}
