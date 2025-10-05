#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace lilToon
{
    public partial class lilToonInspector
    {
        public static lilToonPreset[] presets;
        protected static MaterialEditor m_MaterialEditor;
        protected static RenderingMode renderingModeBuf;
        protected static TransparentMode transparentModeBuf;
        protected static bool isLite = false;
        protected static bool isCutout = false;
        protected static bool isTransparent = false;
        protected static bool isOutl = false;
        protected static bool isRefr = false;
        protected static bool isBlur = false;
        protected static bool isFur = false;
        protected static bool isStWr = false;
        protected static bool isTess = false;
        protected static bool isGem = false;
        protected static bool isFakeShadow = false;
        protected static bool isOnePass = false;
        protected static bool isTwoPass = false;
        protected static bool isMulti = false;
        protected static bool isUseAlpha = false;
        protected static bool isShowRenderMode = true;
        private static lilToonSetting shaderSetting;

        private static readonly lilToonVersion latestVersion = new()
            { latest_vertion_name = "", latest_vertion_value = 0 };

        private static readonly Dictionary<string, MaterialProperty> copiedProperties = new();
        private static bool isCustomEditor;
        private static bool isMultiVariants;
        private readonly Gradient emi2Grad = new();
        private readonly Gradient emiGrad = new();
        private readonly Gradient mainGrad = new();

        private Material[] materials;
        public static lilToonEditorSetting edSet => lilToonEditorSetting.instance;

        protected static GUIStyle boxOuter
        {
            get => lilEditorGUI.boxOuter;
            private set => lilEditorGUI.boxOuter = value;
        }

        protected static GUIStyle boxInnerHalf
        {
            get => lilEditorGUI.boxInnerHalf;
            private set => lilEditorGUI.boxInnerHalf = value;
        }

        protected static GUIStyle boxInner
        {
            get => lilEditorGUI.boxInner;
            private set => lilEditorGUI.boxInner = value;
        }

        protected static GUIStyle customBox
        {
            get => lilEditorGUI.customBox;
            private set => lilEditorGUI.customBox = value;
        }

        protected static GUIStyle customToggleFont
        {
            get => lilEditorGUI.customToggleFont;
            private set => lilEditorGUI.customToggleFont = value;
        }

        protected static GUIStyle wrapLabel
        {
            get => lilEditorGUI.wrapLabel;
            private set => lilEditorGUI.wrapLabel = value;
        }

        protected static GUIStyle boldLabel
        {
            get => lilEditorGUI.boldLabel;
            private set => lilEditorGUI.boldLabel = value;
        }

        protected static GUIStyle foldout
        {
            get => lilEditorGUI.foldout;
            private set => lilEditorGUI.foldout = value;
        }

        protected static GUIStyle middleButton
        {
            get => lilEditorGUI.middleButton;
            private set => lilEditorGUI.middleButton = value;
        }

        protected static string sMainColorBranch
        {
            get => lilLanguageManager.sMainColorBranch;
            private set => lilLanguageManager.sMainColorBranch = value;
        }

        protected static string sCullModes
        {
            get => lilLanguageManager.sCullModes;
            private set => lilLanguageManager.sCullModes = value;
        }

        protected static string sBlendModes
        {
            get => lilLanguageManager.sBlendModes;
            private set => lilLanguageManager.sBlendModes = value;
        }

        protected static string sAlphaModes
        {
            get => lilLanguageManager.sAlphaModes;
            private set => lilLanguageManager.sAlphaModes = value;
        }

        protected static string sAlphaMaskModes
        {
            get => lilLanguageManager.sAlphaMaskModes;
            private set => lilLanguageManager.sAlphaMaskModes = value;
        }

        protected static string blinkSetting
        {
            get => lilLanguageManager.blinkSetting;
            private set => lilLanguageManager.blinkSetting = value;
        }

        protected static string sDistanceFadeSetting
        {
            get => lilLanguageManager.sDistanceFadeSetting;
            private set => lilLanguageManager.sDistanceFadeSetting = value;
        }

        protected static string sDistanceFadeSettingMode
        {
            get => lilLanguageManager.sDistanceFadeSettingMode;
            private set => lilLanguageManager.sDistanceFadeSettingMode = value;
        }

        protected static string sDissolveParams
        {
            get => lilLanguageManager.sDissolveParams;
            private set => lilLanguageManager.sDissolveParams = value;
        }

        protected static string sDissolveParamsMode
        {
            get => lilLanguageManager.sDissolveParamsMode;
            private set => lilLanguageManager.sDissolveParamsMode = value;
        }

        protected static string sDissolveParamsOther
        {
            get => lilLanguageManager.sDissolveParamsOther;
            private set => lilLanguageManager.sDissolveParamsOther = value;
        }

        protected static string sGlitterParams1
        {
            get => lilLanguageManager.sGlitterParams1;
            private set => lilLanguageManager.sGlitterParams1 = value;
        }

        protected static string sGlitterParams2
        {
            get => lilLanguageManager.sGlitterParams2;
            private set => lilLanguageManager.sGlitterParams2 = value;
        }

        protected static string sTransparentMode
        {
            get => lilLanguageManager.sTransparentMode;
            private set => lilLanguageManager.sTransparentMode = value;
        }

        protected static string sOutlineVertexColorUsages
        {
            get => lilLanguageManager.sOutlineVertexColorUsages;
            private set => lilLanguageManager.sOutlineVertexColorUsages = value;
        }

        protected static string sShadowColorTypes
        {
            get => lilLanguageManager.sShadowColorTypes;
            private set => lilLanguageManager.sShadowColorTypes = value;
        }

        protected static string sShadowMaskTypes
        {
            get => lilLanguageManager.sShadowMaskTypes;
            private set => lilLanguageManager.sShadowMaskTypes = value;
        }

        protected static string[] sRenderingModeList
        {
            get => lilLanguageManager.sRenderingModeList;
            private set => lilLanguageManager.sRenderingModeList = value;
        }

        protected static string[] sRenderingModeListLite
        {
            get => lilLanguageManager.sRenderingModeListLite;
            private set => lilLanguageManager.sRenderingModeListLite = value;
        }

        protected static string[] sTransparentModeList
        {
            get => lilLanguageManager.sTransparentModeList;
            private set => lilLanguageManager.sTransparentModeList = value;
        }

        protected static string[] sBlendModeList
        {
            get => lilLanguageManager.sBlendModeList;
            private set => lilLanguageManager.sBlendModeList = value;
        }

        protected static GUIContent mainColorRGBAContent
        {
            get => lilLanguageManager.mainColorRGBAContent;
            private set => lilLanguageManager.mainColorRGBAContent = value;
        }

        protected static GUIContent colorRGBAContent
        {
            get => lilLanguageManager.colorRGBAContent;
            private set => lilLanguageManager.colorRGBAContent = value;
        }

        protected static GUIContent colorAlphaRGBAContent
        {
            get => lilLanguageManager.colorAlphaRGBAContent;
            private set => lilLanguageManager.colorAlphaRGBAContent = value;
        }

        protected static GUIContent maskBlendContent
        {
            get => lilLanguageManager.maskBlendContent;
            private set => lilLanguageManager.maskBlendContent = value;
        }

        protected static GUIContent maskBlendRGBContent
        {
            get => lilLanguageManager.maskBlendRGBContent;
            private set => lilLanguageManager.maskBlendRGBContent = value;
        }

        protected static GUIContent maskBlendRGBAContent
        {
            get => lilLanguageManager.maskBlendRGBAContent;
            private set => lilLanguageManager.maskBlendRGBAContent = value;
        }

        protected static GUIContent colorMaskRGBAContent
        {
            get => lilLanguageManager.colorMaskRGBAContent;
            private set => lilLanguageManager.colorMaskRGBAContent = value;
        }

        protected static GUIContent alphaMaskContent
        {
            get => lilLanguageManager.alphaMaskContent;
            private set => lilLanguageManager.alphaMaskContent = value;
        }

        protected static GUIContent ditherContent
        {
            get => lilLanguageManager.ditherContent;
            private set => lilLanguageManager.ditherContent = value;
        }

        protected static GUIContent maskStrengthContent
        {
            get => lilLanguageManager.maskStrengthContent;
            private set => lilLanguageManager.maskStrengthContent = value;
        }

        protected static GUIContent normalMapContent
        {
            get => lilLanguageManager.normalMapContent;
            private set => lilLanguageManager.normalMapContent = value;
        }

        protected static GUIContent noiseMaskContent
        {
            get => lilLanguageManager.noiseMaskContent;
            private set => lilLanguageManager.noiseMaskContent = value;
        }

        protected static GUIContent matcapContent
        {
            get => lilLanguageManager.matcapContent;
            private set => lilLanguageManager.matcapContent = value;
        }

        protected static GUIContent gradationContent
        {
            get => lilLanguageManager.gradationContent;
            private set => lilLanguageManager.gradationContent = value;
        }

        protected static GUIContent gradSpeedContent
        {
            get => lilLanguageManager.gradSpeedContent;
            private set => lilLanguageManager.gradSpeedContent = value;
        }

        protected static GUIContent smoothnessContent
        {
            get => lilLanguageManager.smoothnessContent;
            private set => lilLanguageManager.smoothnessContent = value;
        }

        protected static GUIContent metallicContent
        {
            get => lilLanguageManager.metallicContent;
            private set => lilLanguageManager.metallicContent = value;
        }

        protected static GUIContent parallaxContent
        {
            get => lilLanguageManager.parallaxContent;
            private set => lilLanguageManager.parallaxContent = value;
        }

        protected static GUIContent audioLinkMaskContent
        {
            get => lilLanguageManager.audioLinkMaskContent;
            private set => lilLanguageManager.audioLinkMaskContent = value;
        }

        protected static GUIContent audioLinkMaskSpectrumContent
        {
            get => lilLanguageManager.audioLinkMaskSpectrumContent;
            private set => lilLanguageManager.audioLinkMaskSpectrumContent = value;
        }

        protected static GUIContent customMaskContent
        {
            get => lilLanguageManager.customMaskContent;
            private set => lilLanguageManager.customMaskContent = value;
        }

        protected static GUIContent shadow1stColorRGBAContent
        {
            get => lilLanguageManager.shadow1stColorRGBAContent;
            private set => lilLanguageManager.shadow1stColorRGBAContent = value;
        }

        protected static GUIContent shadow2ndColorRGBAContent
        {
            get => lilLanguageManager.shadow2ndColorRGBAContent;
            private set => lilLanguageManager.shadow2ndColorRGBAContent = value;
        }

        protected static GUIContent shadow3rdColorRGBAContent
        {
            get => lilLanguageManager.shadow3rdColorRGBAContent;
            private set => lilLanguageManager.shadow3rdColorRGBAContent = value;
        }

        protected static GUIContent blurMaskRGBContent
        {
            get => lilLanguageManager.blurMaskRGBContent;
            private set => lilLanguageManager.blurMaskRGBContent = value;
        }

        protected static GUIContent shadowAOMapContent
        {
            get => lilLanguageManager.shadowAOMapContent;
            private set => lilLanguageManager.shadowAOMapContent = value;
        }

        protected static GUIContent widthMaskContent
        {
            get => lilLanguageManager.widthMaskContent;
            private set => lilLanguageManager.widthMaskContent = value;
        }

        protected static GUIContent lengthMaskContent
        {
            get => lilLanguageManager.lengthMaskContent;
            private set => lilLanguageManager.lengthMaskContent = value;
        }

        protected static GUIContent triMaskContent
        {
            get => lilLanguageManager.triMaskContent;
            private set => lilLanguageManager.triMaskContent = value;
        }

        protected static GUIContent cubemapContent
        {
            get => lilLanguageManager.cubemapContent;
            private set => lilLanguageManager.cubemapContent = value;
        }

        protected static GUIContent audioLinkLocalMapContent
        {
            get => lilLanguageManager.audioLinkLocalMapContent;
            private set => lilLanguageManager.audioLinkLocalMapContent = value;
        }

        protected static GUIContent gradationMapContent
        {
            get => lilLanguageManager.gradationMapContent;
            private set => lilLanguageManager.gradationMapContent = value;
        }

        //------------------------------------------------------------------------------------------------------------------------------
        // Editor variables
        public class lilToonEditorSetting : ScriptableSingleton<lilToonEditorSetting>
        {
            public EditorMode editorMode = EditorMode.Simple;
            public bool isShowBase;
            public bool isShowPrePreset;
            public bool isShowMainUV;
            public bool isShowMain;
            public bool isShowMainTone;
            public bool isShowShadow;
            public bool isShowShadowAO;
            public bool isShowRimShade;
            public bool isShowBump;
            public bool isShowReflections;
            public bool isShowEmission;
            public bool isShowEmissionMap;
            public bool isShowEmissionBlendMask;
            public bool isShowEmission2ndMap;
            public bool isShowEmission2ndBlendMask;
            public bool isShowMatCapUV;
            public bool isShowMatCap2ndUV;
            public bool isShowParallax;
            public bool isShowDistanceFade;
            public bool isShowAudioLink;
            public bool isShowDissolve;
            public bool isShowMain2ndDissolveMask;
            public bool isShowMain2ndDissolveNoiseMask;
            public bool isShowMain3rdDissolveMask;
            public bool isShowMain3rdDissolveNoiseMask;
            public bool isShowBumpMap;
            public bool isShowBump2ndMap;
            public bool isShowBump2ndScaleMask;
            public bool isShowAnisotropyTangentMap;
            public bool isShowAnisotropyScaleMask;
            public bool isShowSmoothnessTex;
            public bool isShowMetallicGlossMap;
            public bool isShowBacklight;
            public bool isShowBacklightColorTex;
            public bool isShowReflectionColorTex;
            public bool isShowMatCap;
            public bool isShowMatCapBlendMask;
            public bool isShowMatCapBumpMap;
            public bool isShowMatCap2ndBlendMask;
            public bool isShowMatCap2ndBumpMap;
            public bool isShowRim;
            public bool isShowRimColorTex;
            public bool isShowGlitter;
            public bool isShowGlitterColorTex;
            public bool isShowGlitterShapeTex;
            public bool isShowGem;
            public bool isShowAudioLinkMask;
            public bool isShowDissolveMask;
            public bool isShowDissolveNoiseMask;
            public bool isShowIDMask;
            public bool isShowUDIMDiscard;
            public bool isShowEncryption;
            public bool isShowStencil;
            public bool isShowOutline;
            public bool isShowOutlineMap;
            public bool isShowRefraction;
            public bool isShowFur;
            public bool isShowTess;
            public bool isShowRendering;
            public bool isShowLightBake;
            public bool isShowOptimization;
            public bool isShowBlend;
            public bool isShowBlendAdd;
            public bool isShowBlendPre;
            public bool isShowBlendAddPre;
            public bool isShowBlendOutline;
            public bool isShowBlendAddOutline;
            public bool isShowBlendFur;
            public bool isShowBlendAddFur;
            public bool isShowWebPages;
            public bool isShowHelpPages;
            public bool isShowLightingSettings;
            public bool isShowShaderSetting;
            public bool isShowOptimizationSetting;
            public bool isShowDefaultValueSetting;
            public bool isShowVRChat;
            public bool isAlphaMaskModeAdvanced;

            public bool[] isShowCategorys = new bool[(int)lilPresetCategory.Other + 1]
                { false, false, false, false, false, false, false };

            public string searchKeyWord = "";
        }

        [Serializable]
        public class lilToonVersion
        {
            public string latest_vertion_name;
            public int latest_vertion_value;
        }

        public struct PropertyBlockData
        {
            public PropertyBlock propertyBlock;
            public bool shouldCopyTex;
        }
    }
}
#endif