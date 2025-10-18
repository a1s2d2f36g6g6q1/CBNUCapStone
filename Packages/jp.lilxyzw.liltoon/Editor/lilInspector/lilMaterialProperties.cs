#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine.Pool;

namespace lilToon
{
    public partial class lilToonInspector
    {
        private readonly lilMaterialProperty aaStrength = new("_AAStrength", PropertyBlock.Base);
        private readonly lilMaterialProperty alphaBoostFA = new("_AlphaBoostFA", PropertyBlock.Lighting);

        private readonly lilMaterialProperty alphaMask = new("_AlphaMask", true, PropertyBlock.MainColor,
            PropertyBlock.AlphaMask);

        private readonly lilMaterialProperty alphaMaskMode =
            new("_AlphaMaskMode", PropertyBlock.MainColor, PropertyBlock.AlphaMask);

        private readonly lilMaterialProperty alphaMaskScale =
            new("_AlphaMaskScale", PropertyBlock.MainColor, PropertyBlock.AlphaMask);

        private readonly lilMaterialProperty alphaMaskValue =
            new("_AlphaMaskValue", PropertyBlock.MainColor, PropertyBlock.AlphaMask);

        private readonly lilMaterialProperty alphaToMask = new("_AlphaToMask", PropertyBlock.Rendering);

        private readonly lilMaterialProperty anisotropy2MatCap =
            new("_Anisotropy2MatCap", PropertyBlock.NormalMap, PropertyBlock.Anisotropy);

        private readonly lilMaterialProperty anisotropy2MatCap2nd =
            new("_Anisotropy2MatCap2nd", PropertyBlock.NormalMap, PropertyBlock.Anisotropy);

        private readonly lilMaterialProperty anisotropy2ndBitangentWidth = new("_Anisotropy2ndBitangentWidth",
            PropertyBlock.NormalMap, PropertyBlock.Anisotropy);

        private readonly lilMaterialProperty anisotropy2ndShift =
            new("_Anisotropy2ndShift", PropertyBlock.NormalMap, PropertyBlock.Anisotropy);

        private readonly lilMaterialProperty anisotropy2ndShiftNoiseScale = new("_Anisotropy2ndShiftNoiseScale",
            PropertyBlock.NormalMap, PropertyBlock.Anisotropy);

        private readonly lilMaterialProperty anisotropy2ndSpecularStrength = new("_Anisotropy2ndSpecularStrength",
            PropertyBlock.NormalMap, PropertyBlock.Anisotropy);

        private readonly lilMaterialProperty anisotropy2ndTangentWidth =
            new("_Anisotropy2ndTangentWidth", PropertyBlock.NormalMap, PropertyBlock.Anisotropy);

        private readonly lilMaterialProperty anisotropy2Reflection =
            new("_Anisotropy2Reflection", PropertyBlock.NormalMap, PropertyBlock.Anisotropy);

        private readonly lilMaterialProperty anisotropyBitangentWidth =
            new("_AnisotropyBitangentWidth", PropertyBlock.NormalMap, PropertyBlock.Anisotropy);

        private readonly lilMaterialProperty anisotropyScale =
            new("_AnisotropyScale", PropertyBlock.NormalMap, PropertyBlock.Anisotropy);

        private readonly lilMaterialProperty anisotropyScaleMask =
            new("_AnisotropyScaleMask", true, PropertyBlock.NormalMap, PropertyBlock.Anisotropy);

        private readonly lilMaterialProperty anisotropyShift =
            new("_AnisotropyShift", PropertyBlock.NormalMap, PropertyBlock.Anisotropy);

        private readonly lilMaterialProperty anisotropyShiftNoiseMask = new("_AnisotropyShiftNoiseMask", true,
            PropertyBlock.NormalMap, PropertyBlock.Anisotropy);

        private readonly lilMaterialProperty anisotropyShiftNoiseScale =
            new("_AnisotropyShiftNoiseScale", PropertyBlock.NormalMap, PropertyBlock.Anisotropy);

        private readonly lilMaterialProperty anisotropySpecularStrength = new("_AnisotropySpecularStrength",
            PropertyBlock.NormalMap, PropertyBlock.Anisotropy);

        private readonly lilMaterialProperty anisotropyTangentMap =
            new("_AnisotropyTangentMap", true, PropertyBlock.NormalMap, PropertyBlock.Anisotropy);

        private readonly lilMaterialProperty anisotropyTangentWidth =
            new("_AnisotropyTangentWidth", PropertyBlock.NormalMap, PropertyBlock.Anisotropy);

        private readonly lilMaterialProperty applyReflection = new("_ApplyReflection", PropertyBlock.Reflection);
        private readonly lilMaterialProperty applySpecular = new("_ApplySpecular", PropertyBlock.Reflection);
        private readonly lilMaterialProperty applySpecularFA = new("_ApplySpecularFA", PropertyBlock.Reflection);
        private readonly lilMaterialProperty asOverlay = new("_AsOverlay", PropertyBlock.Base);

        private readonly lilMaterialProperty asUnlit = new("_AsUnlit", PropertyBlock.Lighting);
        private readonly lilMaterialProperty audioLink2Emission = new("_AudioLink2Emission", PropertyBlock.AudioLink);

        private readonly lilMaterialProperty audioLink2Emission2nd =
            new("_AudioLink2Emission2nd", PropertyBlock.AudioLink);

        private readonly lilMaterialProperty audioLink2Emission2ndGrad =
            new("_AudioLink2Emission2ndGrad", PropertyBlock.AudioLink);

        private readonly lilMaterialProperty audioLink2EmissionGrad =
            new("_AudioLink2EmissionGrad", PropertyBlock.AudioLink);

        private readonly lilMaterialProperty audioLink2Main2nd = new("_AudioLink2Main2nd", PropertyBlock.AudioLink);
        private readonly lilMaterialProperty audioLink2Main3rd = new("_AudioLink2Main3rd", PropertyBlock.AudioLink);
        private readonly lilMaterialProperty audioLink2Vertex = new("_AudioLink2Vertex", PropertyBlock.AudioLink);
        private readonly lilMaterialProperty audioLinkAsLocal = new("_AudioLinkAsLocal", PropertyBlock.AudioLink);

        private readonly lilMaterialProperty audioLinkDefaultValue =
            new("_AudioLinkDefaultValue", PropertyBlock.AudioLink);

        private readonly lilMaterialProperty audioLinkLocalMap = new("_AudioLinkLocalMap", PropertyBlock.AudioLink);

        private readonly lilMaterialProperty audioLinkLocalMapParams =
            new("_AudioLinkLocalMapParams", PropertyBlock.AudioLink);

        private readonly lilMaterialProperty audioLinkMask = new("_AudioLinkMask", true, PropertyBlock.AudioLink);

        private readonly lilMaterialProperty audioLinkMask_ScrollRotate =
            new("_AudioLinkMask_ScrollRotate", true, PropertyBlock.AudioLink);

        private readonly lilMaterialProperty audioLinkMask_UVMode =
            new("_AudioLinkMask_UVMode", true, PropertyBlock.AudioLink);

        private readonly lilMaterialProperty audioLinkStart = new("_AudioLinkStart", PropertyBlock.AudioLink);
        private readonly lilMaterialProperty audioLinkUVMode = new("_AudioLinkUVMode", PropertyBlock.AudioLink);
        private readonly lilMaterialProperty audioLinkUVParams = new("_AudioLinkUVParams", PropertyBlock.AudioLink);

        private readonly lilMaterialProperty audioLinkVertexStart =
            new("_AudioLinkVertexStart", PropertyBlock.AudioLink);

        private readonly lilMaterialProperty audioLinkVertexStrength =
            new("_AudioLinkVertexStrength", PropertyBlock.AudioLink);

        private readonly lilMaterialProperty audioLinkVertexUVMode =
            new("_AudioLinkVertexUVMode", PropertyBlock.AudioLink);

        private readonly lilMaterialProperty audioLinkVertexUVParams =
            new("_AudioLinkVertexUVParams", PropertyBlock.AudioLink);

        private readonly lilMaterialProperty backfaceColor = new("_BackfaceColor", PropertyBlock.Base);
        private readonly lilMaterialProperty backfaceForceShadow = new("_BackfaceForceShadow", PropertyBlock.Base);

        private readonly lilMaterialProperty backlightBackfaceMask =
            new("_BacklightBackfaceMask", PropertyBlock.Backlight);

        private readonly lilMaterialProperty backlightBlur = new("_BacklightBlur", PropertyBlock.Backlight);
        private readonly lilMaterialProperty backlightBorder = new("_BacklightBorder", PropertyBlock.Backlight);
        private readonly lilMaterialProperty backlightColor = new("_BacklightColor", PropertyBlock.Backlight);

        private readonly lilMaterialProperty backlightColorTex =
            new("_BacklightColorTex", true, PropertyBlock.Backlight);

        private readonly lilMaterialProperty backlightDirectivity =
            new("_BacklightDirectivity", PropertyBlock.Backlight);

        private readonly lilMaterialProperty backlightMainStrength =
            new("_BacklightMainStrength", PropertyBlock.Backlight);

        private readonly lilMaterialProperty backlightNormalStrength =
            new("_BacklightNormalStrength", PropertyBlock.Backlight);

        private readonly lilMaterialProperty backlightReceiveShadow =
            new("_BacklightReceiveShadow", PropertyBlock.Backlight);

        private readonly lilMaterialProperty backlightViewStrength =
            new("_BacklightViewStrength", PropertyBlock.Backlight);

        private readonly lilMaterialProperty baseColor = new("_BaseColor");
        private readonly lilMaterialProperty baseColorMap = new("_BaseColorMap", true);
        private readonly lilMaterialProperty baseMap = new("_BaseMap", true);

        private readonly lilMaterialProperty beforeExposureLimit =
            new("_BeforeExposureLimit", PropertyBlock.Lighting, PropertyBlock.Rendering);

        private readonly lilMaterialProperty bitKey0 = new("_BitKey0", PropertyBlock.Encryption);
        private readonly lilMaterialProperty bitKey1 = new("_BitKey1", PropertyBlock.Encryption);
        private readonly lilMaterialProperty bitKey10 = new("_BitKey10", PropertyBlock.Encryption);
        private readonly lilMaterialProperty bitKey11 = new("_BitKey11", PropertyBlock.Encryption);
        private readonly lilMaterialProperty bitKey12 = new("_BitKey12", PropertyBlock.Encryption);
        private readonly lilMaterialProperty bitKey13 = new("_BitKey13", PropertyBlock.Encryption);
        private readonly lilMaterialProperty bitKey14 = new("_BitKey14", PropertyBlock.Encryption);
        private readonly lilMaterialProperty bitKey15 = new("_BitKey15", PropertyBlock.Encryption);
        private readonly lilMaterialProperty bitKey16 = new("_BitKey16", PropertyBlock.Encryption);
        private readonly lilMaterialProperty bitKey17 = new("_BitKey17", PropertyBlock.Encryption);
        private readonly lilMaterialProperty bitKey18 = new("_BitKey18", PropertyBlock.Encryption);
        private readonly lilMaterialProperty bitKey19 = new("_BitKey19", PropertyBlock.Encryption);
        private readonly lilMaterialProperty bitKey2 = new("_BitKey2", PropertyBlock.Encryption);
        private readonly lilMaterialProperty bitKey20 = new("_BitKey20", PropertyBlock.Encryption);
        private readonly lilMaterialProperty bitKey21 = new("_BitKey21", PropertyBlock.Encryption);
        private readonly lilMaterialProperty bitKey22 = new("_BitKey22", PropertyBlock.Encryption);
        private readonly lilMaterialProperty bitKey23 = new("_BitKey23", PropertyBlock.Encryption);
        private readonly lilMaterialProperty bitKey24 = new("_BitKey24", PropertyBlock.Encryption);
        private readonly lilMaterialProperty bitKey25 = new("_BitKey25", PropertyBlock.Encryption);
        private readonly lilMaterialProperty bitKey26 = new("_BitKey26", PropertyBlock.Encryption);
        private readonly lilMaterialProperty bitKey27 = new("_BitKey27", PropertyBlock.Encryption);
        private readonly lilMaterialProperty bitKey28 = new("_BitKey28", PropertyBlock.Encryption);
        private readonly lilMaterialProperty bitKey29 = new("_BitKey29", PropertyBlock.Encryption);
        private readonly lilMaterialProperty bitKey3 = new("_BitKey3", PropertyBlock.Encryption);
        private readonly lilMaterialProperty bitKey30 = new("_BitKey30", PropertyBlock.Encryption);
        private readonly lilMaterialProperty bitKey31 = new("_BitKey31", PropertyBlock.Encryption);
        private readonly lilMaterialProperty bitKey4 = new("_BitKey4", PropertyBlock.Encryption);
        private readonly lilMaterialProperty bitKey5 = new("_BitKey5", PropertyBlock.Encryption);
        private readonly lilMaterialProperty bitKey6 = new("_BitKey6", PropertyBlock.Encryption);
        private readonly lilMaterialProperty bitKey7 = new("_BitKey7", PropertyBlock.Encryption);
        private readonly lilMaterialProperty bitKey8 = new("_BitKey8", PropertyBlock.Encryption);
        private readonly lilMaterialProperty bitKey9 = new("_BitKey9", PropertyBlock.Encryption);
        private readonly lilMaterialProperty blendOp = new("_BlendOp", PropertyBlock.Rendering);
        private readonly lilMaterialProperty blendOpAlpha = new("_BlendOpAlpha", PropertyBlock.Rendering);
        private readonly lilMaterialProperty blendOpAlphaFA = new("_BlendOpAlphaFA", PropertyBlock.Rendering);

        private readonly lilMaterialProperty blendOpFA = new("_BlendOpFA", PropertyBlock.Rendering,
            PropertyBlock.Lighting);

        private readonly lilMaterialProperty bump2ndMap = new("_Bump2ndMap", true, PropertyBlock.NormalMap,
            PropertyBlock.NormalMap2nd);

        private readonly lilMaterialProperty bump2ndMap_UVMode =
            new("_Bump2ndMap_UVMode", PropertyBlock.NormalMap, PropertyBlock.NormalMap2nd);

        private readonly lilMaterialProperty bump2ndScale =
            new("_Bump2ndScale", PropertyBlock.NormalMap, PropertyBlock.NormalMap2nd);

        private readonly lilMaterialProperty bump2ndScaleMask =
            new("_Bump2ndScaleMask", true, PropertyBlock.NormalMap, PropertyBlock.NormalMap2nd);

        private readonly lilMaterialProperty bumpMap = new("_BumpMap", true, PropertyBlock.NormalMap,
            PropertyBlock.NormalMap1st);

        private readonly lilMaterialProperty bumpScale = new("_BumpScale", PropertyBlock.NormalMap,
            PropertyBlock.NormalMap1st);

        private readonly lilMaterialProperty colorMask = new("_ColorMask", PropertyBlock.Rendering);
        private readonly lilMaterialProperty cull = new("_Cull", PropertyBlock.Rendering, PropertyBlock.Base);
        private readonly lilMaterialProperty cutoff = new("_Cutoff", PropertyBlock.Base);
        private readonly lilMaterialProperty dissolveColor = new("_DissolveColor", PropertyBlock.Dissolve);

        private readonly lilMaterialProperty dissolveMask = new("_DissolveMask", true, PropertyBlock.Dissolve);

        private readonly lilMaterialProperty
            dissolveNoiseMask = new("_DissolveNoiseMask", true, PropertyBlock.Dissolve);

        private readonly lilMaterialProperty dissolveNoiseMask_ScrollRotate =
            new("_DissolveNoiseMask_ScrollRotate", PropertyBlock.Dissolve);

        private readonly lilMaterialProperty dissolveNoiseStrength =
            new("_DissolveNoiseStrength", PropertyBlock.Dissolve);

        private readonly lilMaterialProperty dissolveParams = new("_DissolveParams", PropertyBlock.Dissolve);
        private readonly lilMaterialProperty dissolvePos = new("_DissolvePos", PropertyBlock.Dissolve);

        private readonly lilMaterialProperty distanceFade = new("_DistanceFade", PropertyBlock.DistanceFade);
        private readonly lilMaterialProperty distanceFadeColor = new("_DistanceFadeColor", PropertyBlock.DistanceFade);
        private readonly lilMaterialProperty distanceFadeMode = new("_DistanceFadeMode", PropertyBlock.DistanceFade);

        private readonly lilMaterialProperty distanceFadeRimColor =
            new("_DistanceFadeRimColor", PropertyBlock.DistanceFade);

        private readonly lilMaterialProperty distanceFadeRimFresnelPower =
            new("_DistanceFadeRimFresnelPower", PropertyBlock.DistanceFade);

        private readonly lilMaterialProperty ditherMaxValue = new("_DitherMaxValue", PropertyBlock.Base);
        private readonly lilMaterialProperty ditherTex = new("_DitherTex", PropertyBlock.Base);
        private readonly lilMaterialProperty dstBlend = new("_DstBlend", PropertyBlock.Rendering);
        private readonly lilMaterialProperty dstBlendAlpha = new("_DstBlendAlpha", PropertyBlock.Rendering);
        private readonly lilMaterialProperty dstBlendAlphaFA = new("_DstBlendAlphaFA", PropertyBlock.Rendering);
        private readonly lilMaterialProperty dstBlendFA = new("_DstBlendFA", PropertyBlock.Rendering);

        private readonly lilMaterialProperty emission2ndBlend =
            new("_Emission2ndBlend", PropertyBlock.Emission, PropertyBlock.Emission2nd);

        private readonly lilMaterialProperty emission2ndBlendMask =
            new("_Emission2ndBlendMask", true, PropertyBlock.Emission, PropertyBlock.Emission2nd);

        private readonly lilMaterialProperty emission2ndBlendMask_ScrollRotate =
            new("_Emission2ndBlendMask_ScrollRotate", PropertyBlock.Emission, PropertyBlock.Emission2nd);

        private readonly lilMaterialProperty emission2ndBlendMode =
            new("_Emission2ndBlendMode", PropertyBlock.Emission, PropertyBlock.Emission2nd);

        private readonly lilMaterialProperty emission2ndBlink =
            new("_Emission2ndBlink", PropertyBlock.Emission, PropertyBlock.Emission2nd);

        private readonly lilMaterialProperty emission2ndColor =
            new("_Emission2ndColor", PropertyBlock.Emission, PropertyBlock.Emission2nd);

        private readonly lilMaterialProperty emission2ndFluorescence =
            new("_Emission2ndFluorescence", PropertyBlock.Emission, PropertyBlock.Emission2nd);

        private readonly lilMaterialProperty emission2ndGradSpeed =
            new("_Emission2ndGradSpeed", PropertyBlock.Emission, PropertyBlock.Emission2nd);

        private readonly lilMaterialProperty emission2ndGradTex =
            new("_Emission2ndGradTex", true, PropertyBlock.Emission, PropertyBlock.Emission2nd);

        private readonly lilMaterialProperty emission2ndMainStrength =
            new("_Emission2ndMainStrength", PropertyBlock.Emission, PropertyBlock.Emission2nd);

        private readonly lilMaterialProperty emission2ndMap =
            new("_Emission2ndMap", true, PropertyBlock.Emission, PropertyBlock.Emission2nd);

        private readonly lilMaterialProperty emission2ndMap_ScrollRotate = new("_Emission2ndMap_ScrollRotate",
            PropertyBlock.Emission, PropertyBlock.Emission2nd);

        private readonly lilMaterialProperty emission2ndMap_UVMode =
            new("_Emission2ndMap_UVMode", PropertyBlock.Emission, PropertyBlock.Emission2nd);

        private readonly lilMaterialProperty emission2ndParallaxDepth =
            new("_Emission2ndParallaxDepth", PropertyBlock.Emission, PropertyBlock.Emission2nd);

        private readonly lilMaterialProperty emission2ndUseGrad =
            new("_Emission2ndUseGrad", PropertyBlock.Emission, PropertyBlock.Emission2nd);

        private readonly lilMaterialProperty emissionBlend =
            new("_EmissionBlend", PropertyBlock.Emission, PropertyBlock.Emission1st);

        private readonly lilMaterialProperty emissionBlendMask =
            new("_EmissionBlendMask", true, PropertyBlock.Emission, PropertyBlock.Emission1st);

        private readonly lilMaterialProperty emissionBlendMask_ScrollRotate = new("_EmissionBlendMask_ScrollRotate",
            PropertyBlock.Emission, PropertyBlock.Emission1st);

        private readonly lilMaterialProperty emissionBlendMode =
            new("_EmissionBlendMode", PropertyBlock.Emission, PropertyBlock.Emission1st);

        private readonly lilMaterialProperty emissionBlink =
            new("_EmissionBlink", PropertyBlock.Emission, PropertyBlock.Emission1st);

        private readonly lilMaterialProperty emissionColor =
            new("_EmissionColor", PropertyBlock.Emission, PropertyBlock.Emission1st);

        private readonly lilMaterialProperty emissionFluorescence =
            new("_EmissionFluorescence", PropertyBlock.Emission, PropertyBlock.Emission1st);

        private readonly lilMaterialProperty emissionGradSpeed =
            new("_EmissionGradSpeed", PropertyBlock.Emission, PropertyBlock.Emission1st);

        private readonly lilMaterialProperty emissionGradTex =
            new("_EmissionGradTex", true, PropertyBlock.Emission, PropertyBlock.Emission1st);

        private readonly lilMaterialProperty emissionMainStrength =
            new("_EmissionMainStrength", PropertyBlock.Emission, PropertyBlock.Emission1st);

        private readonly lilMaterialProperty emissionMap = new("_EmissionMap", true, PropertyBlock.Emission,
            PropertyBlock.Emission1st);

        private readonly lilMaterialProperty emissionMap_ScrollRotate =
            new("_EmissionMap_ScrollRotate", PropertyBlock.Emission, PropertyBlock.Emission1st);

        private readonly lilMaterialProperty emissionMap_UVMode =
            new("_EmissionMap_UVMode", PropertyBlock.Emission, PropertyBlock.Emission1st);

        private readonly lilMaterialProperty emissionParallaxDepth =
            new("_EmissionParallaxDepth", PropertyBlock.Emission, PropertyBlock.Emission1st);

        private readonly lilMaterialProperty emissionUseGrad =
            new("_EmissionUseGrad", PropertyBlock.Emission, PropertyBlock.Emission1st);

        private readonly lilMaterialProperty fakeShadowVector = new("_FakeShadowVector", PropertyBlock.Base);
        private readonly lilMaterialProperty flipNormal = new("_FlipNormal", PropertyBlock.Base);
        private readonly lilMaterialProperty furAlphaToMask = new("_FurAlphaToMask", PropertyBlock.Rendering);
        private readonly lilMaterialProperty furAO = new("_FurAO", PropertyBlock.Fur);
        private readonly lilMaterialProperty furBlendOp = new("_FurBlendOp", PropertyBlock.Rendering);
        private readonly lilMaterialProperty furBlendOpAlpha = new("_FurBlendOpAlpha", PropertyBlock.Rendering);
        private readonly lilMaterialProperty furBlendOpAlphaFA = new("_FurBlendOpAlphaFA", PropertyBlock.Rendering);
        private readonly lilMaterialProperty furBlendOpFA = new("_FurBlendOpFA", PropertyBlock.Rendering);
        private readonly lilMaterialProperty furColorMask = new("_FurColorMask", PropertyBlock.Rendering);

        private readonly lilMaterialProperty furCull = new("_FurCull", PropertyBlock.Rendering);
        private readonly lilMaterialProperty furCutoutLength = new("_FurCutoutLength", PropertyBlock.Fur);
        private readonly lilMaterialProperty furDstBlend = new("_FurDstBlend", PropertyBlock.Rendering);
        private readonly lilMaterialProperty furDstBlendAlpha = new("_FurDstBlendAlpha", PropertyBlock.Rendering);
        private readonly lilMaterialProperty furDstBlendAlphaFA = new("_FurDstBlendAlphaFA", PropertyBlock.Rendering);
        private readonly lilMaterialProperty furDstBlendFA = new("_FurDstBlendFA", PropertyBlock.Rendering);
        private readonly lilMaterialProperty furGravity = new("_FurGravity", PropertyBlock.Fur);
        private readonly lilMaterialProperty furLayerNum = new("_FurLayerNum", PropertyBlock.Fur);
        private readonly lilMaterialProperty furLengthMask = new("_FurLengthMask", true, PropertyBlock.Fur);
        private readonly lilMaterialProperty furMask = new("_FurMask", true, PropertyBlock.Fur);
        private readonly lilMaterialProperty furMeshType = new("_FurMeshType", PropertyBlock.Fur);

        private readonly lilMaterialProperty furNoiseMask = new("_FurNoiseMask", true, PropertyBlock.Fur);
        private readonly lilMaterialProperty furOffsetFactor = new("_FurOffsetFactor", PropertyBlock.Rendering);
        private readonly lilMaterialProperty furOffsetUnits = new("_FurOffsetUnits", PropertyBlock.Rendering);
        private readonly lilMaterialProperty furRandomize = new("_FurRandomize", PropertyBlock.Fur);
        private readonly lilMaterialProperty furRimAntiLight = new("_FurRimAntiLight", PropertyBlock.Fur);
        private readonly lilMaterialProperty furRimColor = new("_FurRimColor", PropertyBlock.Fur);
        private readonly lilMaterialProperty furRimFresnelPower = new("_FurRimFresnelPower", PropertyBlock.Fur);
        private readonly lilMaterialProperty furRootOffset = new("_FurRootOffset", PropertyBlock.Fur);
        private readonly lilMaterialProperty furSrcBlend = new("_FurSrcBlend", PropertyBlock.Rendering);
        private readonly lilMaterialProperty furSrcBlendAlpha = new("_FurSrcBlendAlpha", PropertyBlock.Rendering);
        private readonly lilMaterialProperty furSrcBlendAlphaFA = new("_FurSrcBlendAlphaFA", PropertyBlock.Rendering);
        private readonly lilMaterialProperty furSrcBlendFA = new("_FurSrcBlendFA", PropertyBlock.Rendering);
        private readonly lilMaterialProperty furStencilComp = new("_FurStencilComp", PropertyBlock.Stencil);
        private readonly lilMaterialProperty furStencilFail = new("_FurStencilFail", PropertyBlock.Stencil);
        private readonly lilMaterialProperty furStencilPass = new("_FurStencilPass", PropertyBlock.Stencil);
        private readonly lilMaterialProperty furStencilReadMask = new("_FurStencilReadMask", PropertyBlock.Stencil);
        private readonly lilMaterialProperty furStencilRef = new("_FurStencilRef", PropertyBlock.Stencil);
        private readonly lilMaterialProperty furStencilWriteMask = new("_FurStencilWriteMask", PropertyBlock.Stencil);
        private readonly lilMaterialProperty furStencilZFail = new("_FurStencilZFail", PropertyBlock.Stencil);
        private readonly lilMaterialProperty furTouchStrength = new("_FurTouchStrength", PropertyBlock.Fur);
        private readonly lilMaterialProperty furVector = new("_FurVector", PropertyBlock.Fur);
        private readonly lilMaterialProperty furVectorScale = new("_FurVectorScale", PropertyBlock.Fur);
        private readonly lilMaterialProperty furVectorTex = new("_FurVectorTex", true, PropertyBlock.Fur);
        private readonly lilMaterialProperty furZclip = new("_FurZClip", PropertyBlock.Rendering);
        private readonly lilMaterialProperty furZtest = new("_FurZTest", PropertyBlock.Rendering);
        private readonly lilMaterialProperty furZwrite = new("_FurZWrite", PropertyBlock.Rendering);

        private readonly lilMaterialProperty gemChromaticAberration = new("_GemChromaticAberration", PropertyBlock.Gem);
        private readonly lilMaterialProperty gemEnvColor = new("_GemEnvColor", PropertyBlock.Gem);
        private readonly lilMaterialProperty gemEnvContrast = new("_GemEnvContrast", PropertyBlock.Gem);
        private readonly lilMaterialProperty gemParticleColor = new("_GemParticleColor", PropertyBlock.Gem);
        private readonly lilMaterialProperty gemParticleLoop = new("_GemParticleLoop", PropertyBlock.Gem);
        private readonly lilMaterialProperty gemVRParallaxStrength = new("_GemVRParallaxStrength", PropertyBlock.Gem);

        private readonly lilMaterialProperty glitterAngleRandomize =
            new("_GlitterAngleRandomize", PropertyBlock.Glitter);

        private readonly lilMaterialProperty glitterApplyShape = new("_GlitterApplyShape", PropertyBlock.Glitter);

        private readonly lilMaterialProperty glitterApplyTransparency =
            new("_GlitterApplyTransparency", PropertyBlock.Glitter);

        private readonly lilMaterialProperty glitterAtras = new("_GlitterAtras", PropertyBlock.Glitter);
        private readonly lilMaterialProperty glitterBackfaceMask = new("_GlitterBackfaceMask", PropertyBlock.Glitter);
        private readonly lilMaterialProperty glitterColor = new("_GlitterColor", PropertyBlock.Glitter);
        private readonly lilMaterialProperty glitterColorTex = new("_GlitterColorTex", true, PropertyBlock.Glitter);

        private readonly lilMaterialProperty glitterColorTex_UVMode =
            new("_GlitterColorTex_UVMode", true, PropertyBlock.Glitter);

        private readonly lilMaterialProperty glitterEnableLighting =
            new("_GlitterEnableLighting", PropertyBlock.Glitter);

        private readonly lilMaterialProperty glitterMainStrength = new("_GlitterMainStrength", PropertyBlock.Glitter);

        private readonly lilMaterialProperty glitterNormalStrength =
            new("_GlitterNormalStrength", PropertyBlock.Glitter);

        private readonly lilMaterialProperty glitterParams1 = new("_GlitterParams1", PropertyBlock.Glitter);
        private readonly lilMaterialProperty glitterParams2 = new("_GlitterParams2", PropertyBlock.Glitter);
        private readonly lilMaterialProperty glitterPostContrast = new("_GlitterPostContrast", PropertyBlock.Glitter);

        private readonly lilMaterialProperty glitterScaleRandomize =
            new("_GlitterScaleRandomize", PropertyBlock.Glitter);

        private readonly lilMaterialProperty glitterSensitivity = new("_GlitterSensitivity", PropertyBlock.Glitter);
        private readonly lilMaterialProperty glitterShadowMask = new("_GlitterShadowMask", PropertyBlock.Glitter);
        private readonly lilMaterialProperty glitterShapeTex = new("_GlitterShapeTex", true, PropertyBlock.Glitter);
        private readonly lilMaterialProperty glitterUVMode = new("_GlitterUVMode", PropertyBlock.Glitter);

        private readonly lilMaterialProperty glitterVRParallaxStrength =
            new("_GlitterVRParallaxStrength", PropertyBlock.Glitter);

        private readonly lilMaterialProperty gsaaStrength = new("_GSAAStrength", PropertyBlock.Reflection);
        private readonly lilMaterialProperty idMask1 = new("_IDMask1", PropertyBlock.IDMask);
        private readonly lilMaterialProperty idMask2 = new("_IDMask2", PropertyBlock.IDMask);
        private readonly lilMaterialProperty idMask3 = new("_IDMask3", PropertyBlock.IDMask);
        private readonly lilMaterialProperty idMask4 = new("_IDMask4", PropertyBlock.IDMask);
        private readonly lilMaterialProperty idMask5 = new("_IDMask5", PropertyBlock.IDMask);
        private readonly lilMaterialProperty idMask6 = new("_IDMask6", PropertyBlock.IDMask);
        private readonly lilMaterialProperty idMask7 = new("_IDMask7", PropertyBlock.IDMask);
        private readonly lilMaterialProperty idMask8 = new("_IDMask8", PropertyBlock.IDMask);

        private readonly lilMaterialProperty idMaskCompile = new("_IDMaskCompile", PropertyBlock.IDMask);

        private readonly lilMaterialProperty idMaskControlsDissolve =
            new("_IDMaskControlsDissolve", PropertyBlock.IDMask);

        private readonly lilMaterialProperty idMaskFrom = new("_IDMaskFrom", PropertyBlock.IDMask);
        private readonly lilMaterialProperty idMaskIndex1 = new("_IDMaskIndex1", PropertyBlock.IDMask);
        private readonly lilMaterialProperty idMaskIndex2 = new("_IDMaskIndex2", PropertyBlock.IDMask);
        private readonly lilMaterialProperty idMaskIndex3 = new("_IDMaskIndex3", PropertyBlock.IDMask);
        private readonly lilMaterialProperty idMaskIndex4 = new("_IDMaskIndex4", PropertyBlock.IDMask);
        private readonly lilMaterialProperty idMaskIndex5 = new("_IDMaskIndex5", PropertyBlock.IDMask);
        private readonly lilMaterialProperty idMaskIndex6 = new("_IDMaskIndex6", PropertyBlock.IDMask);
        private readonly lilMaterialProperty idMaskIndex7 = new("_IDMaskIndex7", PropertyBlock.IDMask);
        private readonly lilMaterialProperty idMaskIndex8 = new("_IDMaskIndex8", PropertyBlock.IDMask);
        private readonly lilMaterialProperty idMaskIsBitmap = new("_IDMaskIsBitmap", PropertyBlock.IDMask);
        private readonly lilMaterialProperty idMaskPrior1 = new("_IDMaskPrior1", PropertyBlock.IDMask);
        private readonly lilMaterialProperty idMaskPrior2 = new("_IDMaskPrior2", PropertyBlock.IDMask);
        private readonly lilMaterialProperty idMaskPrior3 = new("_IDMaskPrior3", PropertyBlock.IDMask);
        private readonly lilMaterialProperty idMaskPrior4 = new("_IDMaskPrior4", PropertyBlock.IDMask);
        private readonly lilMaterialProperty idMaskPrior5 = new("_IDMaskPrior5", PropertyBlock.IDMask);
        private readonly lilMaterialProperty idMaskPrior6 = new("_IDMaskPrior6", PropertyBlock.IDMask);
        private readonly lilMaterialProperty idMaskPrior7 = new("_IDMaskPrior7", PropertyBlock.IDMask);
        private readonly lilMaterialProperty idMaskPrior8 = new("_IDMaskPrior8", PropertyBlock.IDMask);

        private readonly lilMaterialProperty ignoreEncryption = new("_IgnoreEncryption", PropertyBlock.Encryption);

        //------------------------------------------------------------------------------------------------------------------------------
        // Material properties
        private readonly lilMaterialProperty invisible = new("_Invisible", PropertyBlock.Base);
        private readonly lilMaterialProperty keys = new("_Keys", PropertyBlock.Encryption);

        private readonly lilMaterialProperty lightDirectionOverride =
            new("_LightDirectionOverride", PropertyBlock.Lighting);

        private readonly lilMaterialProperty lightMaxLimit = new("_LightMaxLimit", PropertyBlock.Lighting);
        private readonly lilMaterialProperty lightMinLimit = new("_LightMinLimit", PropertyBlock.Lighting);

        private readonly lilMaterialProperty lilDirectionalLightStrength =
            new("_lilDirectionalLightStrength", PropertyBlock.Lighting);

        private readonly lilMaterialProperty lilShadowCasterBias =
            new("_lilShadowCasterBias", PropertyBlock.Shadow, PropertyBlock.Rendering);

        private readonly lilMaterialProperty main2ndBlendMask =
            new("_Main2ndBlendMask", true, PropertyBlock.MainColor, PropertyBlock.MainColor2nd);

        private readonly lilMaterialProperty main2ndDissolveColor =
            new("_Main2ndDissolveColor", PropertyBlock.MainColor, PropertyBlock.MainColor2nd);

        private readonly lilMaterialProperty main2ndDissolveMask = new("_Main2ndDissolveMask", true,
            PropertyBlock.MainColor, PropertyBlock.MainColor2nd);

        private readonly lilMaterialProperty main2ndDissolveNoiseMask = new("_Main2ndDissolveNoiseMask", true,
            PropertyBlock.MainColor, PropertyBlock.MainColor2nd);

        private readonly lilMaterialProperty main2ndDissolveNoiseMask_ScrollRotate =
            new("_Main2ndDissolveNoiseMask_ScrollRotate", PropertyBlock.MainColor, PropertyBlock.MainColor2nd);

        private readonly lilMaterialProperty main2ndDissolveNoiseStrength = new("_Main2ndDissolveNoiseStrength",
            PropertyBlock.MainColor, PropertyBlock.MainColor2nd);

        private readonly lilMaterialProperty main2ndDissolveParams =
            new("_Main2ndDissolveParams", PropertyBlock.MainColor, PropertyBlock.MainColor2nd);

        private readonly lilMaterialProperty main2ndDissolvePos =
            new("_Main2ndDissolvePos", PropertyBlock.MainColor, PropertyBlock.MainColor2nd);

        private readonly lilMaterialProperty main2ndDistanceFade =
            new("_Main2ndDistanceFade", PropertyBlock.MainColor, PropertyBlock.MainColor2nd);

        private readonly lilMaterialProperty main2ndEnableLighting =
            new("_Main2ndEnableLighting", PropertyBlock.MainColor, PropertyBlock.MainColor2nd);

        private readonly lilMaterialProperty main2ndTex = new("_Main2ndTex", true, PropertyBlock.MainColor,
            PropertyBlock.MainColor2nd);

        private readonly lilMaterialProperty main2ndTex_Cull =
            new("_Main2ndTex_Cull", PropertyBlock.MainColor, PropertyBlock.MainColor2nd);

        private readonly lilMaterialProperty main2ndTex_ScrollRotate =
            new("_Main2ndTex_ScrollRotate", PropertyBlock.MainColor, PropertyBlock.MainColor2nd);

        private readonly lilMaterialProperty main2ndTex_UVMode =
            new("_Main2ndTex_UVMode", PropertyBlock.MainColor, PropertyBlock.MainColor2nd);

        private readonly lilMaterialProperty main2ndTexAlphaMode =
            new("_Main2ndTexAlphaMode", PropertyBlock.MainColor, PropertyBlock.MainColor2nd);

        private readonly lilMaterialProperty main2ndTexAngle =
            new("_Main2ndTexAngle", PropertyBlock.MainColor, PropertyBlock.MainColor2nd);

        private readonly lilMaterialProperty main2ndTexBlendMode =
            new("_Main2ndTexBlendMode", PropertyBlock.MainColor, PropertyBlock.MainColor2nd);

        private readonly lilMaterialProperty main2ndTexDecalAnimation = new("_Main2ndTexDecalAnimation",
            PropertyBlock.MainColor, PropertyBlock.MainColor2nd);

        private readonly lilMaterialProperty main2ndTexDecalSubParam =
            new("_Main2ndTexDecalSubParam", PropertyBlock.MainColor, PropertyBlock.MainColor2nd);

        private readonly lilMaterialProperty main2ndTexIsDecal =
            new("_Main2ndTexIsDecal", PropertyBlock.MainColor, PropertyBlock.MainColor2nd);

        private readonly lilMaterialProperty main2ndTexIsLeftOnly =
            new("_Main2ndTexIsLeftOnly", PropertyBlock.MainColor, PropertyBlock.MainColor2nd);

        private readonly lilMaterialProperty main2ndTexIsMSDF =
            new("_Main2ndTexIsMSDF", PropertyBlock.MainColor, PropertyBlock.MainColor2nd);

        private readonly lilMaterialProperty main2ndTexIsRightOnly =
            new("_Main2ndTexIsRightOnly", PropertyBlock.MainColor, PropertyBlock.MainColor2nd);

        private readonly lilMaterialProperty main2ndTexShouldCopy =
            new("_Main2ndTexShouldCopy", PropertyBlock.MainColor, PropertyBlock.MainColor2nd);

        private readonly lilMaterialProperty main2ndTexShouldFlipCopy = new("_Main2ndTexShouldFlipCopy",
            PropertyBlock.MainColor, PropertyBlock.MainColor2nd);

        private readonly lilMaterialProperty main2ndTexShouldFlipMirror = new("_Main2ndTexShouldFlipMirror",
            PropertyBlock.MainColor, PropertyBlock.MainColor2nd);

        private readonly lilMaterialProperty main3rdBlendMask =
            new("_Main3rdBlendMask", true, PropertyBlock.MainColor, PropertyBlock.MainColor3rd);

        private readonly lilMaterialProperty main3rdDissolveColor =
            new("_Main3rdDissolveColor", PropertyBlock.MainColor, PropertyBlock.MainColor3rd);

        private readonly lilMaterialProperty main3rdDissolveMask = new("_Main3rdDissolveMask", true,
            PropertyBlock.MainColor, PropertyBlock.MainColor3rd);

        private readonly lilMaterialProperty main3rdDissolveNoiseMask = new("_Main3rdDissolveNoiseMask", true,
            PropertyBlock.MainColor, PropertyBlock.MainColor3rd);

        private readonly lilMaterialProperty main3rdDissolveNoiseMask_ScrollRotate =
            new("_Main3rdDissolveNoiseMask_ScrollRotate", PropertyBlock.MainColor, PropertyBlock.MainColor3rd);

        private readonly lilMaterialProperty main3rdDissolveNoiseStrength = new("_Main3rdDissolveNoiseStrength",
            PropertyBlock.MainColor, PropertyBlock.MainColor3rd);

        private readonly lilMaterialProperty main3rdDissolveParams =
            new("_Main3rdDissolveParams", PropertyBlock.MainColor, PropertyBlock.MainColor3rd);

        private readonly lilMaterialProperty main3rdDissolvePos =
            new("_Main3rdDissolvePos", PropertyBlock.MainColor, PropertyBlock.MainColor3rd);

        private readonly lilMaterialProperty main3rdDistanceFade =
            new("_Main3rdDistanceFade", PropertyBlock.MainColor, PropertyBlock.MainColor3rd);

        private readonly lilMaterialProperty main3rdEnableLighting =
            new("_Main3rdEnableLighting", PropertyBlock.MainColor, PropertyBlock.MainColor3rd);

        private readonly lilMaterialProperty main3rdTex = new("_Main3rdTex", true, PropertyBlock.MainColor,
            PropertyBlock.MainColor3rd);

        private readonly lilMaterialProperty main3rdTex_Cull =
            new("_Main3rdTex_Cull", PropertyBlock.MainColor, PropertyBlock.MainColor3rd);

        private readonly lilMaterialProperty main3rdTex_ScrollRotate =
            new("_Main3rdTex_ScrollRotate", PropertyBlock.MainColor, PropertyBlock.MainColor3rd);

        private readonly lilMaterialProperty main3rdTex_UVMode =
            new("_Main3rdTex_UVMode", PropertyBlock.MainColor, PropertyBlock.MainColor3rd);

        private readonly lilMaterialProperty main3rdTexAlphaMode =
            new("_Main3rdTexAlphaMode", PropertyBlock.MainColor, PropertyBlock.MainColor3rd);

        private readonly lilMaterialProperty main3rdTexAngle =
            new("_Main3rdTexAngle", PropertyBlock.MainColor, PropertyBlock.MainColor3rd);

        private readonly lilMaterialProperty main3rdTexBlendMode =
            new("_Main3rdTexBlendMode", PropertyBlock.MainColor, PropertyBlock.MainColor3rd);

        private readonly lilMaterialProperty main3rdTexDecalAnimation = new("_Main3rdTexDecalAnimation",
            PropertyBlock.MainColor, PropertyBlock.MainColor3rd);

        private readonly lilMaterialProperty main3rdTexDecalSubParam =
            new("_Main3rdTexDecalSubParam", PropertyBlock.MainColor, PropertyBlock.MainColor3rd);

        private readonly lilMaterialProperty main3rdTexIsDecal =
            new("_Main3rdTexIsDecal", PropertyBlock.MainColor, PropertyBlock.MainColor3rd);

        private readonly lilMaterialProperty main3rdTexIsLeftOnly =
            new("_Main3rdTexIsLeftOnly", PropertyBlock.MainColor, PropertyBlock.MainColor3rd);

        private readonly lilMaterialProperty main3rdTexIsMSDF =
            new("_Main3rdTexIsMSDF", PropertyBlock.MainColor, PropertyBlock.MainColor3rd);

        private readonly lilMaterialProperty main3rdTexIsRightOnly =
            new("_Main3rdTexIsRightOnly", PropertyBlock.MainColor, PropertyBlock.MainColor3rd);

        private readonly lilMaterialProperty main3rdTexShouldCopy =
            new("_Main3rdTexShouldCopy", PropertyBlock.MainColor, PropertyBlock.MainColor3rd);

        private readonly lilMaterialProperty main3rdTexShouldFlipCopy = new("_Main3rdTexShouldFlipCopy",
            PropertyBlock.MainColor, PropertyBlock.MainColor3rd);

        private readonly lilMaterialProperty main3rdTexShouldFlipMirror = new("_Main3rdTexShouldFlipMirror",
            PropertyBlock.MainColor, PropertyBlock.MainColor3rd);

        private readonly lilMaterialProperty mainColor = new("_Color", PropertyBlock.MainColor,
            PropertyBlock.MainColor1st);

        private readonly lilMaterialProperty mainColor2nd =
            new("_Color2nd", PropertyBlock.MainColor, PropertyBlock.MainColor2nd);

        private readonly lilMaterialProperty mainColor3rd =
            new("_Color3rd", PropertyBlock.MainColor, PropertyBlock.MainColor3rd);

        private readonly lilMaterialProperty mainColorAdjustMask = new("_MainColorAdjustMask", true,
            PropertyBlock.MainColor, PropertyBlock.MainColor1st);

        private readonly lilMaterialProperty mainGradationStrength =
            new("_MainGradationStrength", PropertyBlock.MainColor, PropertyBlock.MainColor1st);

        private readonly lilMaterialProperty mainGradationTex =
            new("_MainGradationTex", true, PropertyBlock.MainColor, PropertyBlock.MainColor1st);

        private readonly lilMaterialProperty mainTex = new("_MainTex", true, PropertyBlock.MainColor,
            PropertyBlock.MainColor1st);

        private readonly lilMaterialProperty mainTex_ScrollRotate = new("_MainTex_ScrollRotate", PropertyBlock.UV);

        private readonly lilMaterialProperty mainTexHSVG = new("_MainTexHSVG", PropertyBlock.MainColor,
            PropertyBlock.MainColor1st);

        private readonly lilMaterialProperty matcap2ndApplyTransparency =
            new("_MatCap2ndApplyTransparency", PropertyBlock.MatCaps, PropertyBlock.MatCap2nd);

        private readonly lilMaterialProperty matcap2ndBackfaceMask =
            new("_MatCap2ndBackfaceMask", PropertyBlock.MatCaps, PropertyBlock.MatCap2nd);

        private readonly lilMaterialProperty matcap2ndBlend =
            new("_MatCap2ndBlend", PropertyBlock.MatCaps, PropertyBlock.MatCap2nd);

        private readonly lilMaterialProperty matcap2ndBlendMask =
            new("_MatCap2ndBlendMask", true, PropertyBlock.MatCaps, PropertyBlock.MatCap2nd);

        private readonly lilMaterialProperty matcap2ndBlendMode =
            new("_MatCap2ndBlendMode", PropertyBlock.MatCaps, PropertyBlock.MatCap2nd);

        private readonly lilMaterialProperty matcap2ndBlendUV1 =
            new("_MatCap2ndBlendUV1", PropertyBlock.MatCaps, PropertyBlock.MatCap2nd);

        private readonly lilMaterialProperty matcap2ndBumpMap =
            new("_MatCap2ndBumpMap", true, PropertyBlock.MatCaps, PropertyBlock.MatCap2nd);

        private readonly lilMaterialProperty matcap2ndBumpScale =
            new("_MatCap2ndBumpScale", PropertyBlock.MatCaps, PropertyBlock.MatCap2nd);

        private readonly lilMaterialProperty matcap2ndColor =
            new("_MatCap2ndColor", PropertyBlock.MatCaps, PropertyBlock.MatCap2nd);

        private readonly lilMaterialProperty matcap2ndCustomNormal =
            new("_MatCap2ndCustomNormal", PropertyBlock.MatCaps, PropertyBlock.MatCap2nd);

        private readonly lilMaterialProperty matcap2ndEnableLighting =
            new("_MatCap2ndEnableLighting", PropertyBlock.MatCaps, PropertyBlock.MatCap2nd);

        private readonly lilMaterialProperty matcap2ndLod =
            new("_MatCap2ndLod", PropertyBlock.MatCaps, PropertyBlock.MatCap2nd);

        private readonly lilMaterialProperty matcap2ndMainStrength =
            new("_MatCap2ndMainStrength", PropertyBlock.MatCaps, PropertyBlock.MatCap2nd);

        private readonly lilMaterialProperty matcap2ndNormalStrength =
            new("_MatCap2ndNormalStrength", PropertyBlock.MatCaps, PropertyBlock.MatCap2nd);

        private readonly lilMaterialProperty matcap2ndPerspective =
            new("_MatCap2ndPerspective", PropertyBlock.MatCaps, PropertyBlock.MatCap2nd);

        private readonly lilMaterialProperty matcap2ndShadowMask =
            new("_MatCap2ndShadowMask", PropertyBlock.MatCaps, PropertyBlock.MatCap2nd);

        private readonly lilMaterialProperty matcap2ndTex =
            new("_MatCap2ndTex", true, PropertyBlock.MatCaps, PropertyBlock.MatCap2nd);

        private readonly lilMaterialProperty matcap2ndVRParallaxStrength =
            new("_MatCap2ndVRParallaxStrength", PropertyBlock.MatCaps, PropertyBlock.MatCap2nd);

        private readonly lilMaterialProperty matcap2ndZRotCancel =
            new("_MatCap2ndZRotCancel", PropertyBlock.MatCaps, PropertyBlock.MatCap2nd);

        private readonly lilMaterialProperty matcapApplyTransparency =
            new("_MatCapApplyTransparency", PropertyBlock.MatCaps, PropertyBlock.MatCap1st);

        private readonly lilMaterialProperty matcapBackfaceMask =
            new("_MatCapBackfaceMask", PropertyBlock.MatCaps, PropertyBlock.MatCap1st);

        private readonly lilMaterialProperty matcapBlend = new("_MatCapBlend", PropertyBlock.MatCaps,
            PropertyBlock.MatCap1st);

        private readonly lilMaterialProperty matcapBlendMask =
            new("_MatCapBlendMask", true, PropertyBlock.MatCaps, PropertyBlock.MatCap1st);

        private readonly lilMaterialProperty matcapBlendMode =
            new("_MatCapBlendMode", PropertyBlock.MatCaps, PropertyBlock.MatCap1st);

        private readonly lilMaterialProperty matcapBlendUV1 =
            new("_MatCapBlendUV1", PropertyBlock.MatCaps, PropertyBlock.MatCap1st);

        private readonly lilMaterialProperty matcapBumpMap =
            new("_MatCapBumpMap", true, PropertyBlock.MatCaps, PropertyBlock.MatCap1st);

        private readonly lilMaterialProperty matcapBumpScale =
            new("_MatCapBumpScale", PropertyBlock.MatCaps, PropertyBlock.MatCap1st);

        private readonly lilMaterialProperty matcapColor = new("_MatCapColor", PropertyBlock.MatCaps,
            PropertyBlock.MatCap1st);

        private readonly lilMaterialProperty matcapCustomNormal =
            new("_MatCapCustomNormal", PropertyBlock.MatCaps, PropertyBlock.MatCap1st);

        private readonly lilMaterialProperty matcapEnableLighting =
            new("_MatCapEnableLighting", PropertyBlock.MatCaps, PropertyBlock.MatCap1st);

        private readonly lilMaterialProperty matcapLod = new("_MatCapLod", PropertyBlock.MatCaps,
            PropertyBlock.MatCap1st);

        private readonly lilMaterialProperty matcapMainStrength =
            new("_MatCapMainStrength", PropertyBlock.MatCaps, PropertyBlock.MatCap1st);

        private readonly lilMaterialProperty matcapMul = new("_MatCapMul", PropertyBlock.MatCaps,
            PropertyBlock.MatCap1st);

        private readonly lilMaterialProperty matcapNormalStrength =
            new("_MatCapNormalStrength", PropertyBlock.MatCaps, PropertyBlock.MatCap1st);

        private readonly lilMaterialProperty matcapPerspective =
            new("_MatCapPerspective", PropertyBlock.MatCaps, PropertyBlock.MatCap1st);

        private readonly lilMaterialProperty matcapShadowMask =
            new("_MatCapShadowMask", PropertyBlock.MatCaps, PropertyBlock.MatCap1st);

        private readonly lilMaterialProperty matcapTex = new("_MatCapTex", true, PropertyBlock.MatCaps,
            PropertyBlock.MatCap1st);

        private readonly lilMaterialProperty matcapVRParallaxStrength =
            new("_MatCapVRParallaxStrength", PropertyBlock.MatCaps, PropertyBlock.MatCap1st);

        private readonly lilMaterialProperty matcapZRotCancel =
            new("_MatCapZRotCancel", PropertyBlock.MatCaps, PropertyBlock.MatCap1st);

        private readonly lilMaterialProperty metallic = new("_Metallic", PropertyBlock.Reflection, PropertyBlock.Gem);

        private readonly lilMaterialProperty metallicGlossMap =
            new("_MetallicGlossMap", true, PropertyBlock.Reflection, PropertyBlock.Gem);

        private readonly lilMaterialProperty monochromeLighting = new("_MonochromeLighting", PropertyBlock.Lighting);
        private readonly lilMaterialProperty offsetFactor = new("_OffsetFactor", PropertyBlock.Rendering);
        private readonly lilMaterialProperty offsetUnits = new("_OffsetUnits", PropertyBlock.Rendering);
        private readonly lilMaterialProperty outlineAlphaToMask = new("_OutlineAlphaToMask", PropertyBlock.Rendering);
        private readonly lilMaterialProperty outlineBlendOp = new("_OutlineBlendOp", PropertyBlock.Rendering);
        private readonly lilMaterialProperty outlineBlendOpAlpha = new("_OutlineBlendOpAlpha", PropertyBlock.Rendering);

        private readonly lilMaterialProperty outlineBlendOpAlphaFA =
            new("_OutlineBlendOpAlphaFA", PropertyBlock.Rendering);

        private readonly lilMaterialProperty outlineBlendOpFA = new("_OutlineBlendOpFA", PropertyBlock.Rendering);

        private readonly lilMaterialProperty outlineColor = new("_OutlineColor", PropertyBlock.Outline);
        private readonly lilMaterialProperty outlineColorMask = new("_OutlineColorMask", PropertyBlock.Rendering);

        private readonly lilMaterialProperty outlineCull = new("_OutlineCull", PropertyBlock.Rendering);
        private readonly lilMaterialProperty outlineDeleteMesh = new("_OutlineDeleteMesh", PropertyBlock.Outline);
        private readonly lilMaterialProperty outlineDisableInVR = new("_OutlineDisableInVR", PropertyBlock.Outline);
        private readonly lilMaterialProperty outlineDstBlend = new("_OutlineDstBlend", PropertyBlock.Rendering);

        private readonly lilMaterialProperty outlineDstBlendAlpha =
            new("_OutlineDstBlendAlpha", PropertyBlock.Rendering);

        private readonly lilMaterialProperty outlineDstBlendAlphaFA =
            new("_OutlineDstBlendAlphaFA", PropertyBlock.Rendering);

        private readonly lilMaterialProperty outlineDstBlendFA = new("_OutlineDstBlendFA", PropertyBlock.Rendering);

        private readonly lilMaterialProperty outlineEnableLighting =
            new("_OutlineEnableLighting", PropertyBlock.Outline);

        private readonly lilMaterialProperty outlineFixWidth = new("_OutlineFixWidth", PropertyBlock.Outline);
        private readonly lilMaterialProperty outlineLitApplyTex = new("_OutlineLitApplyTex", PropertyBlock.Outline);
        private readonly lilMaterialProperty outlineLitColor = new("_OutlineLitColor", PropertyBlock.Outline);
        private readonly lilMaterialProperty outlineLitOffset = new("_OutlineLitOffset", PropertyBlock.Outline);
        private readonly lilMaterialProperty outlineLitScale = new("_OutlineLitScale", PropertyBlock.Outline);

        private readonly lilMaterialProperty outlineLitShadowReceive =
            new("_OutlineLitShadowReceive", PropertyBlock.Outline);

        private readonly lilMaterialProperty outlineOffsetFactor = new("_OutlineOffsetFactor", PropertyBlock.Rendering);
        private readonly lilMaterialProperty outlineOffsetUnits = new("_OutlineOffsetUnits", PropertyBlock.Rendering);
        private readonly lilMaterialProperty outlineSrcBlend = new("_OutlineSrcBlend", PropertyBlock.Rendering);

        private readonly lilMaterialProperty outlineSrcBlendAlpha =
            new("_OutlineSrcBlendAlpha", PropertyBlock.Rendering);

        private readonly lilMaterialProperty outlineSrcBlendAlphaFA =
            new("_OutlineSrcBlendAlphaFA", PropertyBlock.Rendering);

        private readonly lilMaterialProperty outlineSrcBlendFA = new("_OutlineSrcBlendFA", PropertyBlock.Rendering);
        private readonly lilMaterialProperty outlineStencilComp = new("_OutlineStencilComp", PropertyBlock.Stencil);
        private readonly lilMaterialProperty outlineStencilFail = new("_OutlineStencilFail", PropertyBlock.Stencil);
        private readonly lilMaterialProperty outlineStencilPass = new("_OutlineStencilPass", PropertyBlock.Stencil);

        private readonly lilMaterialProperty outlineStencilReadMask =
            new("_OutlineStencilReadMask", PropertyBlock.Stencil);

        private readonly lilMaterialProperty outlineStencilRef = new("_OutlineStencilRef", PropertyBlock.Stencil);

        private readonly lilMaterialProperty outlineStencilWriteMask =
            new("_OutlineStencilWriteMask", PropertyBlock.Stencil);

        private readonly lilMaterialProperty outlineStencilZFail = new("_OutlineStencilZFail", PropertyBlock.Stencil);
        private readonly lilMaterialProperty outlineTex = new("_OutlineTex", true, PropertyBlock.Outline);

        private readonly lilMaterialProperty outlineTex_ScrollRotate =
            new("_OutlineTex_ScrollRotate", PropertyBlock.Outline);

        private readonly lilMaterialProperty outlineTexHSVG = new("_OutlineTexHSVG", PropertyBlock.Outline);
        private readonly lilMaterialProperty outlineVectorScale = new("_OutlineVectorScale", PropertyBlock.Outline);
        private readonly lilMaterialProperty outlineVectorTex = new("_OutlineVectorTex", true, PropertyBlock.Outline);
        private readonly lilMaterialProperty outlineVectorUVMode = new("_OutlineVectorUVMode", PropertyBlock.Outline);
        private readonly lilMaterialProperty outlineVertexR2Width = new("_OutlineVertexR2Width", PropertyBlock.Outline);
        private readonly lilMaterialProperty outlineWidth = new("_OutlineWidth", PropertyBlock.Outline);
        private readonly lilMaterialProperty outlineWidthMask = new("_OutlineWidthMask", true, PropertyBlock.Outline);
        private readonly lilMaterialProperty outlineZBias = new("_OutlineZBias", PropertyBlock.Outline);
        private readonly lilMaterialProperty outlineZclip = new("_OutlineZClip", PropertyBlock.Rendering);
        private readonly lilMaterialProperty outlineZtest = new("_OutlineZTest", PropertyBlock.Rendering);
        private readonly lilMaterialProperty outlineZwrite = new("_OutlineZWrite", PropertyBlock.Rendering);
        private readonly lilMaterialProperty parallax = new("_Parallax", PropertyBlock.Parallax);
        private readonly lilMaterialProperty parallaxMap = new("_ParallaxMap", true, PropertyBlock.Parallax);
        private readonly lilMaterialProperty parallaxOffset = new("_ParallaxOffset", PropertyBlock.Parallax);
        private readonly lilMaterialProperty preAlphaToMask = new("_PreAlphaToMask", PropertyBlock.Rendering);
        private readonly lilMaterialProperty preBlendOp = new("_PreBlendOp", PropertyBlock.Rendering);
        private readonly lilMaterialProperty preBlendOpAlpha = new("_PreBlendOpAlpha", PropertyBlock.Rendering);
        private readonly lilMaterialProperty preBlendOpAlphaFA = new("_PreBlendOpAlphaFA", PropertyBlock.Rendering);
        private readonly lilMaterialProperty preBlendOpFA = new("_PreBlendOpFA", PropertyBlock.Rendering);
        private readonly lilMaterialProperty preColor = new("_PreColor", PropertyBlock.Base);
        private readonly lilMaterialProperty preColorMask = new("_PreColorMask", PropertyBlock.Rendering);

        private readonly lilMaterialProperty preCull = new("_PreCull", PropertyBlock.Rendering);
        private readonly lilMaterialProperty preCutoff = new("_PreCutoff", PropertyBlock.Base);
        private readonly lilMaterialProperty preDstBlend = new("_PreDstBlend", PropertyBlock.Rendering);
        private readonly lilMaterialProperty preDstBlendAlpha = new("_PreDstBlendAlpha", PropertyBlock.Rendering);
        private readonly lilMaterialProperty preDstBlendAlphaFA = new("_PreDstBlendAlphaFA", PropertyBlock.Rendering);
        private readonly lilMaterialProperty preDstBlendFA = new("_PreDstBlendFA", PropertyBlock.Rendering);
        private readonly lilMaterialProperty preOffsetFactor = new("_PreOffsetFactor", PropertyBlock.Rendering);
        private readonly lilMaterialProperty preOffsetUnits = new("_PreOffsetUnits", PropertyBlock.Rendering);
        private readonly lilMaterialProperty preOutType = new("_PreOutType", PropertyBlock.Base);
        private readonly lilMaterialProperty preSrcBlend = new("_PreSrcBlend", PropertyBlock.Rendering);
        private readonly lilMaterialProperty preSrcBlendAlpha = new("_PreSrcBlendAlpha", PropertyBlock.Rendering);
        private readonly lilMaterialProperty preSrcBlendAlphaFA = new("_PreSrcBlendAlphaFA", PropertyBlock.Rendering);
        private readonly lilMaterialProperty preSrcBlendFA = new("_PreSrcBlendFA", PropertyBlock.Rendering);
        private readonly lilMaterialProperty preStencilComp = new("_PreStencilComp", PropertyBlock.Stencil);
        private readonly lilMaterialProperty preStencilFail = new("_PreStencilFail", PropertyBlock.Stencil);
        private readonly lilMaterialProperty preStencilPass = new("_PreStencilPass", PropertyBlock.Stencil);
        private readonly lilMaterialProperty preStencilReadMask = new("_PreStencilReadMask", PropertyBlock.Stencil);
        private readonly lilMaterialProperty preStencilRef = new("_PreStencilRef", PropertyBlock.Stencil);
        private readonly lilMaterialProperty preStencilWriteMask = new("_PreStencilWriteMask", PropertyBlock.Stencil);
        private readonly lilMaterialProperty preStencilZFail = new("_PreStencilZFail", PropertyBlock.Stencil);
        private readonly lilMaterialProperty preZclip = new("_PreZClip", PropertyBlock.Rendering);
        private readonly lilMaterialProperty preZtest = new("_PreZTest", PropertyBlock.Rendering);
        private readonly lilMaterialProperty preZwrite = new("_PreZWrite", PropertyBlock.Rendering);
        private readonly lilMaterialProperty reflectance = new("_Reflectance", PropertyBlock.Reflection);

        private readonly lilMaterialProperty reflectionApplyTransparency =
            new("_ReflectionApplyTransparency", PropertyBlock.Reflection);

        private readonly lilMaterialProperty
            reflectionBlendMode = new("_ReflectionBlendMode", PropertyBlock.Reflection);

        private readonly lilMaterialProperty reflectionColor = new("_ReflectionColor", PropertyBlock.Reflection);

        private readonly lilMaterialProperty reflectionColorTex =
            new("_ReflectionColorTex", true, PropertyBlock.Reflection);

        private readonly lilMaterialProperty reflectionCubeColor =
            new("_ReflectionCubeColor", true, PropertyBlock.Reflection);

        private readonly lilMaterialProperty reflectionCubeEnableLighting =
            new("_ReflectionCubeEnableLighting", PropertyBlock.Reflection);

        private readonly lilMaterialProperty reflectionCubeOverride =
            new("_ReflectionCubeOverride", PropertyBlock.Reflection);

        private readonly lilMaterialProperty reflectionCubeTex = new("_ReflectionCubeTex", PropertyBlock.Reflection);

        private readonly lilMaterialProperty reflectionNormalStrength =
            new("_ReflectionNormalStrength", PropertyBlock.Reflection);

        private readonly lilMaterialProperty refractionColor = new("_RefractionColor", PropertyBlock.Refraction);

        private readonly lilMaterialProperty refractionColorFromMain =
            new("_RefractionColorFromMain", PropertyBlock.Refraction);

        private readonly lilMaterialProperty refractionFresnelPower =
            new("_RefractionFresnelPower", PropertyBlock.Refraction, PropertyBlock.Gem);

        private readonly lilMaterialProperty refractionStrength =
            new("_RefractionStrength", PropertyBlock.Refraction, PropertyBlock.Gem);

        private readonly lilMaterialProperty
            rimApplyTransparency = new("_RimApplyTransparency", PropertyBlock.RimLight);

        private readonly lilMaterialProperty rimBackfaceMask = new("_RimBackfaceMask", PropertyBlock.RimLight);
        private readonly lilMaterialProperty rimBlendMode = new("_RimBlendMode", PropertyBlock.RimLight);
        private readonly lilMaterialProperty rimBlur = new("_RimBlur", PropertyBlock.RimLight);
        private readonly lilMaterialProperty rimBorder = new("_RimBorder", PropertyBlock.RimLight);
        private readonly lilMaterialProperty rimColor = new("_RimColor", PropertyBlock.RimLight);
        private readonly lilMaterialProperty rimColorTex = new("_RimColorTex", true, PropertyBlock.RimLight);
        private readonly lilMaterialProperty rimDirRange = new("_RimDirRange", PropertyBlock.RimLight);
        private readonly lilMaterialProperty rimDirStrength = new("_RimDirStrength", PropertyBlock.RimLight);
        private readonly lilMaterialProperty rimEnableLighting = new("_RimEnableLighting", PropertyBlock.RimLight);
        private readonly lilMaterialProperty rimFresnelPower = new("_RimFresnelPower", PropertyBlock.RimLight);
        private readonly lilMaterialProperty rimIndirBlur = new("_RimIndirBlur", PropertyBlock.RimLight);
        private readonly lilMaterialProperty rimIndirBorder = new("_RimIndirBorder", PropertyBlock.RimLight);
        private readonly lilMaterialProperty rimIndirColor = new("_RimIndirColor", PropertyBlock.RimLight);
        private readonly lilMaterialProperty rimIndirRange = new("_RimIndirRange", PropertyBlock.RimLight);
        private readonly lilMaterialProperty rimMainStrength = new("_RimMainStrength", PropertyBlock.RimLight);
        private readonly lilMaterialProperty rimNormalStrength = new("_RimNormalStrength", PropertyBlock.RimLight);
        private readonly lilMaterialProperty rimShadeBlur = new("_RimShadeBlur", PropertyBlock.RimShade);
        private readonly lilMaterialProperty rimShadeBorder = new("_RimShadeBorder", PropertyBlock.RimShade);
        private readonly lilMaterialProperty rimShadeColor = new("_RimShadeColor", PropertyBlock.RimShade);

        private readonly lilMaterialProperty
            rimShadeFresnelPower = new("_RimShadeFresnelPower", PropertyBlock.RimShade);

        private readonly lilMaterialProperty rimShadeMask = new("_RimShadeMask", PropertyBlock.RimShade);

        private readonly lilMaterialProperty rimShadeNormalStrength =
            new("_RimShadeNormalStrength", PropertyBlock.RimShade);

        private readonly lilMaterialProperty rimShadowMask = new("_RimShadowMask", PropertyBlock.RimLight);

        private readonly lilMaterialProperty rimVRParallaxStrength =
            new("_RimVRParallaxStrength", PropertyBlock.RimLight);

        private readonly lilMaterialProperty shadow2ndBlur = new("_Shadow2ndBlur", PropertyBlock.Shadow);
        private readonly lilMaterialProperty shadow2ndBorder = new("_Shadow2ndBorder", PropertyBlock.Shadow);
        private readonly lilMaterialProperty shadow2ndColor = new("_Shadow2ndColor", PropertyBlock.Shadow);
        private readonly lilMaterialProperty shadow2ndColorTex = new("_Shadow2ndColorTex", true, PropertyBlock.Shadow);

        private readonly lilMaterialProperty shadow2ndNormalStrength =
            new("_Shadow2ndNormalStrength", PropertyBlock.Shadow);

        private readonly lilMaterialProperty shadow2ndReceive = new("_Shadow2ndReceive", PropertyBlock.Shadow);
        private readonly lilMaterialProperty shadow3rdBlur = new("_Shadow3rdBlur", PropertyBlock.Shadow);
        private readonly lilMaterialProperty shadow3rdBorder = new("_Shadow3rdBorder", PropertyBlock.Shadow);
        private readonly lilMaterialProperty shadow3rdColor = new("_Shadow3rdColor", PropertyBlock.Shadow);
        private readonly lilMaterialProperty shadow3rdColorTex = new("_Shadow3rdColorTex", true, PropertyBlock.Shadow);

        private readonly lilMaterialProperty shadow3rdNormalStrength =
            new("_Shadow3rdNormalStrength", PropertyBlock.Shadow);

        private readonly lilMaterialProperty shadow3rdReceive = new("_Shadow3rdReceive", PropertyBlock.Shadow);
        private readonly lilMaterialProperty shadowAOShift = new("_ShadowAOShift", PropertyBlock.Shadow);
        private readonly lilMaterialProperty shadowAOShift2 = new("_ShadowAOShift2", PropertyBlock.Shadow);
        private readonly lilMaterialProperty shadowBlur = new("_ShadowBlur", PropertyBlock.Shadow);
        private readonly lilMaterialProperty shadowBlurMask = new("_ShadowBlurMask", true, PropertyBlock.Shadow);
        private readonly lilMaterialProperty shadowBlurMaskLOD = new("_ShadowBlurMaskLOD", PropertyBlock.Shadow);
        private readonly lilMaterialProperty shadowBorder = new("_ShadowBorder", PropertyBlock.Shadow);
        private readonly lilMaterialProperty shadowBorderColor = new("_ShadowBorderColor", PropertyBlock.Shadow);
        private readonly lilMaterialProperty shadowBorderMask = new("_ShadowBorderMask", true, PropertyBlock.Shadow);
        private readonly lilMaterialProperty shadowBorderMaskLOD = new("_ShadowBorderMaskLOD", PropertyBlock.Shadow);
        private readonly lilMaterialProperty shadowBorderRange = new("_ShadowBorderRange", PropertyBlock.Shadow);
        private readonly lilMaterialProperty shadowColor = new("_ShadowColor", PropertyBlock.Shadow);
        private readonly lilMaterialProperty shadowColorTex = new("_ShadowColorTex", true, PropertyBlock.Shadow);
        private readonly lilMaterialProperty shadowColorType = new("_ShadowColorType", PropertyBlock.Shadow);

        private readonly lilMaterialProperty shadowEnvStrength =
            new("_ShadowEnvStrength", PropertyBlock.Shadow, PropertyBlock.Lighting);

        private readonly lilMaterialProperty shadowFlatBlur = new("_ShadowFlatBlur", PropertyBlock.Shadow);
        private readonly lilMaterialProperty shadowFlatBorder = new("_ShadowFlatBorder", PropertyBlock.Shadow);
        private readonly lilMaterialProperty shadowMainStrength = new("_ShadowMainStrength", PropertyBlock.Shadow);
        private readonly lilMaterialProperty shadowMaskType = new("_ShadowMaskType", PropertyBlock.Shadow);
        private readonly lilMaterialProperty shadowNormalStrength = new("_ShadowNormalStrength", PropertyBlock.Shadow);
        private readonly lilMaterialProperty shadowPostAO = new("_ShadowPostAO", PropertyBlock.Shadow);
        private readonly lilMaterialProperty shadowReceive = new("_ShadowReceive", PropertyBlock.Shadow);
        private readonly lilMaterialProperty shadowStrength = new("_ShadowStrength", PropertyBlock.Shadow);

        private readonly lilMaterialProperty
            shadowStrengthMask = new("_ShadowStrengthMask", true, PropertyBlock.Shadow);

        private readonly lilMaterialProperty
            shadowStrengthMaskLOD = new("_ShadowStrengthMaskLOD", PropertyBlock.Shadow);

        private readonly lilMaterialProperty shiftBackfaceUV = new("_ShiftBackfaceUV", PropertyBlock.UV);
        private readonly lilMaterialProperty smoothness = new("_Smoothness", PropertyBlock.Reflection);
        private readonly lilMaterialProperty smoothnessTex = new("_SmoothnessTex", true, PropertyBlock.Reflection);
        private readonly lilMaterialProperty specularBlur = new("_SpecularBlur", PropertyBlock.Reflection);
        private readonly lilMaterialProperty specularBorder = new("_SpecularBorder", PropertyBlock.Reflection);

        private readonly lilMaterialProperty specularNormalStrength =
            new("_SpecularNormalStrength", PropertyBlock.Reflection);

        private readonly lilMaterialProperty specularToon = new("_SpecularToon", PropertyBlock.Reflection);
        private readonly lilMaterialProperty srcBlend = new("_SrcBlend", PropertyBlock.Rendering);
        private readonly lilMaterialProperty srcBlendAlpha = new("_SrcBlendAlpha", PropertyBlock.Rendering);
        private readonly lilMaterialProperty srcBlendAlphaFA = new("_SrcBlendAlphaFA", PropertyBlock.Rendering);
        private readonly lilMaterialProperty srcBlendFA = new("_SrcBlendFA", PropertyBlock.Rendering);
        private readonly lilMaterialProperty stencilComp = new("_StencilComp", PropertyBlock.Stencil);
        private readonly lilMaterialProperty stencilFail = new("_StencilFail", PropertyBlock.Stencil);
        private readonly lilMaterialProperty stencilPass = new("_StencilPass", PropertyBlock.Stencil);
        private readonly lilMaterialProperty stencilReadMask = new("_StencilReadMask", PropertyBlock.Stencil);

        private readonly lilMaterialProperty stencilRef = new("_StencilRef", PropertyBlock.Stencil);
        private readonly lilMaterialProperty stencilWriteMask = new("_StencilWriteMask", PropertyBlock.Stencil);
        private readonly lilMaterialProperty stencilZFail = new("_StencilZFail", PropertyBlock.Stencil);

        private readonly lilMaterialProperty subpassCutoff = new("_SubpassCutoff", PropertyBlock.Rendering);

        private readonly lilMaterialProperty tessEdge = new("_TessEdge", PropertyBlock.Tessellation);
        private readonly lilMaterialProperty tessFactorMax = new("_TessFactorMax", PropertyBlock.Tessellation);
        private readonly lilMaterialProperty tessShrink = new("_TessShrink", PropertyBlock.Tessellation);
        private readonly lilMaterialProperty tessStrength = new("_TessStrength", PropertyBlock.Tessellation);

        private readonly lilMaterialProperty transparentModeMat = new("_TransparentMode", PropertyBlock.Base);
        private readonly lilMaterialProperty triMask = new("_TriMask", true, PropertyBlock.Base);

        private readonly lilMaterialProperty udimDiscardCompile = new("_UDIMDiscardCompile", PropertyBlock.UDIMDiscard);
        private readonly lilMaterialProperty udimDiscardMethod = new("_UDIMDiscardMode", PropertyBlock.UDIMDiscard);
        private readonly lilMaterialProperty udimDiscardRow0_0 = new("_UDIMDiscardRow0_0", PropertyBlock.UDIMDiscard);
        private readonly lilMaterialProperty udimDiscardRow0_1 = new("_UDIMDiscardRow0_1", PropertyBlock.UDIMDiscard);
        private readonly lilMaterialProperty udimDiscardRow0_2 = new("_UDIMDiscardRow0_2", PropertyBlock.UDIMDiscard);
        private readonly lilMaterialProperty udimDiscardRow0_3 = new("_UDIMDiscardRow0_3", PropertyBlock.UDIMDiscard);
        private readonly lilMaterialProperty udimDiscardRow1_0 = new("_UDIMDiscardRow1_0", PropertyBlock.UDIMDiscard);
        private readonly lilMaterialProperty udimDiscardRow1_1 = new("_UDIMDiscardRow1_1", PropertyBlock.UDIMDiscard);
        private readonly lilMaterialProperty udimDiscardRow1_2 = new("_UDIMDiscardRow1_2", PropertyBlock.UDIMDiscard);
        private readonly lilMaterialProperty udimDiscardRow1_3 = new("_UDIMDiscardRow1_3", PropertyBlock.UDIMDiscard);
        private readonly lilMaterialProperty udimDiscardRow2_0 = new("_UDIMDiscardRow2_0", PropertyBlock.UDIMDiscard);
        private readonly lilMaterialProperty udimDiscardRow2_1 = new("_UDIMDiscardRow2_1", PropertyBlock.UDIMDiscard);
        private readonly lilMaterialProperty udimDiscardRow2_2 = new("_UDIMDiscardRow2_2", PropertyBlock.UDIMDiscard);
        private readonly lilMaterialProperty udimDiscardRow2_3 = new("_UDIMDiscardRow2_3", PropertyBlock.UDIMDiscard);
        private readonly lilMaterialProperty udimDiscardRow3_0 = new("_UDIMDiscardRow3_0", PropertyBlock.UDIMDiscard);
        private readonly lilMaterialProperty udimDiscardRow3_1 = new("_UDIMDiscardRow3_1", PropertyBlock.UDIMDiscard);
        private readonly lilMaterialProperty udimDiscardRow3_2 = new("_UDIMDiscardRow3_2", PropertyBlock.UDIMDiscard);
        private readonly lilMaterialProperty udimDiscardRow3_3 = new("_UDIMDiscardRow3_3", PropertyBlock.UDIMDiscard);
        private readonly lilMaterialProperty udimDiscardUV = new("_UDIMDiscardUV", PropertyBlock.UDIMDiscard);

        private readonly lilMaterialProperty useAnisotropy =
            new("_UseAnisotropy", PropertyBlock.NormalMap, PropertyBlock.Anisotropy);

        private readonly lilMaterialProperty useAudioLink = new("_UseAudioLink", PropertyBlock.AudioLink);

        private readonly lilMaterialProperty useBacklight = new("_UseBacklight", PropertyBlock.Backlight);

        private readonly lilMaterialProperty useBump2ndMap =
            new("_UseBump2ndMap", PropertyBlock.NormalMap, PropertyBlock.NormalMap2nd);

        private readonly lilMaterialProperty useBumpMap = new("_UseBumpMap", PropertyBlock.NormalMap,
            PropertyBlock.NormalMap1st);

        private readonly lilMaterialProperty useClippingCanceller = new("_UseClippingCanceller", PropertyBlock.Base);
        private readonly lilMaterialProperty useDither = new("_UseDither", PropertyBlock.Base);

        private readonly lilMaterialProperty useEmission = new("_UseEmission", PropertyBlock.Emission,
            PropertyBlock.Emission1st);

        private readonly lilMaterialProperty useEmission2nd =
            new("_UseEmission2nd", PropertyBlock.Emission, PropertyBlock.Emission2nd);

        private readonly lilMaterialProperty useGlitter = new("_UseGlitter", PropertyBlock.Glitter);

        private readonly lilMaterialProperty useMain2ndTex =
            new("_UseMain2ndTex", PropertyBlock.MainColor, PropertyBlock.MainColor2nd);

        private readonly lilMaterialProperty useMain3rdTex =
            new("_UseMain3rdTex", PropertyBlock.MainColor, PropertyBlock.MainColor3rd);

        private readonly lilMaterialProperty useMatCap = new("_UseMatCap", PropertyBlock.MatCaps,
            PropertyBlock.MatCap1st);

        private readonly lilMaterialProperty useMatCap2nd =
            new("_UseMatCap2nd", PropertyBlock.MatCaps, PropertyBlock.MatCap2nd);

        private readonly lilMaterialProperty useParallax = new("_UseParallax", PropertyBlock.Parallax);
        private readonly lilMaterialProperty usePOM = new("_UsePOM", PropertyBlock.Parallax);

        private readonly lilMaterialProperty useReflection = new("_UseReflection", PropertyBlock.Reflection);

        private readonly lilMaterialProperty useRim = new("_UseRim", PropertyBlock.RimLight);

        private readonly lilMaterialProperty useRimShade = new("_UseRimShade", PropertyBlock.RimShade);

        private readonly lilMaterialProperty useShadow = new("_UseShadow", PropertyBlock.Shadow);
        private readonly lilMaterialProperty vertexColor2FurVector = new("_VertexColor2FurVector", PropertyBlock.Fur);
        private readonly lilMaterialProperty vertexLightStrength = new("_VertexLightStrength", PropertyBlock.Lighting);
        private readonly lilMaterialProperty zclip = new("_ZClip", PropertyBlock.Rendering);
        private readonly lilMaterialProperty ztest = new("_ZTest", PropertyBlock.Rendering);
        private readonly lilMaterialProperty zwrite = new("_ZWrite", PropertyBlock.Rendering, PropertyBlock.Base);

        private lilMaterialProperty[] allProperty;

        private Dictionary<PropertyBlock, List<lilMaterialProperty>> block2Propertes;

        private lilMaterialProperty[] AllProperties()
        {
            return allProperty ??= new[]
            {
                invisible,
                cutoff,
                preColor,
                preOutType,
                preCutoff,
                flipNormal,
                backfaceForceShadow,
                backfaceColor,
                aaStrength,
                useDither,
                ditherTex,
                ditherMaxValue,

                asUnlit,
                vertexLightStrength,
                lightMinLimit,
                lightMaxLimit,
                beforeExposureLimit,
                monochromeLighting,
                alphaBoostFA,
                lilDirectionalLightStrength,
                lightDirectionOverride,

                baseColor,
                baseMap,
                baseColorMap,

                shiftBackfaceUV,
                mainTex_ScrollRotate,

                mainColor,
                mainTex,
                mainTexHSVG,
                mainGradationStrength,
                mainGradationTex,
                mainColorAdjustMask,

                useMain2ndTex,
                mainColor2nd,
                main2ndTex,
                main2ndTexAngle,
                main2ndTex_ScrollRotate,
                main2ndTex_UVMode,
                main2ndTex_Cull,
                main2ndTexDecalAnimation,
                main2ndTexDecalSubParam,
                main2ndTexIsDecal,
                main2ndTexIsLeftOnly,
                main2ndTexIsRightOnly,
                main2ndTexShouldCopy,
                main2ndTexShouldFlipMirror,
                main2ndTexShouldFlipCopy,
                main2ndTexIsMSDF,
                main2ndBlendMask,
                main2ndTexBlendMode,
                main2ndTexAlphaMode,
                main2ndEnableLighting,
                main2ndDissolveMask,
                main2ndDissolveNoiseMask,
                main2ndDissolveNoiseMask_ScrollRotate,
                main2ndDissolveNoiseStrength,
                main2ndDissolveColor,
                main2ndDissolveParams,
                main2ndDissolvePos,
                main2ndDistanceFade,

                useMain3rdTex,
                mainColor3rd,
                main3rdTex,
                main3rdTexAngle,
                main3rdTex_ScrollRotate,
                main3rdTex_UVMode,
                main3rdTex_Cull,
                main3rdTexDecalAnimation,
                main3rdTexDecalSubParam,
                main3rdTexIsDecal,
                main3rdTexIsLeftOnly,
                main3rdTexIsRightOnly,
                main3rdTexShouldCopy,
                main3rdTexShouldFlipMirror,
                main3rdTexShouldFlipCopy,
                main3rdTexIsMSDF,
                main3rdBlendMask,
                main3rdTexBlendMode,
                main3rdTexAlphaMode,
                main3rdEnableLighting,
                main3rdDissolveMask,
                main3rdDissolveNoiseMask,
                main3rdDissolveNoiseMask_ScrollRotate,
                main3rdDissolveNoiseStrength,
                main3rdDissolveColor,
                main3rdDissolveParams,
                main3rdDissolvePos,
                main3rdDistanceFade,

                alphaMaskMode,
                alphaMask,
                alphaMaskScale,
                alphaMaskValue,

                useShadow,
                shadowStrength,
                shadowStrengthMask,
                shadowBorderMask,
                shadowBlurMask,
                shadowStrengthMaskLOD,
                shadowBorderMaskLOD,
                shadowBlurMaskLOD,
                shadowAOShift,
                shadowAOShift2,
                shadowPostAO,
                shadowColorType,
                shadowColor,
                shadowColorTex,
                shadowNormalStrength,
                shadowBorder,
                shadowBlur,
                shadow2ndColor,
                shadow2ndColorTex,
                shadow2ndNormalStrength,
                shadow2ndBorder,
                shadow2ndBlur,
                shadow3rdColor,
                shadow3rdColorTex,
                shadow3rdNormalStrength,
                shadow3rdBorder,
                shadow3rdBlur,
                shadowMainStrength,
                shadowEnvStrength,
                shadowBorderColor,
                shadowBorderRange,
                shadowReceive,
                shadow2ndReceive,
                shadow3rdReceive,
                shadowMaskType,
                shadowFlatBorder,
                shadowFlatBlur,
                lilShadowCasterBias,

                useRimShade,
                rimShadeColor,
                rimShadeMask,
                rimShadeNormalStrength,
                rimShadeBorder,
                rimShadeBlur,
                rimShadeFresnelPower,

                useEmission,
                emissionColor,
                emissionMap,
                emissionMap_ScrollRotate,
                emissionMap_UVMode,
                emissionMainStrength,
                emissionBlend,
                emissionBlendMask,
                emissionBlendMask_ScrollRotate,
                emissionBlendMode,
                emissionBlink,
                emissionUseGrad,
                emissionGradTex,
                emissionGradSpeed,
                emissionParallaxDepth,
                emissionFluorescence,

                useEmission2nd,
                emission2ndColor,
                emission2ndMap,
                emission2ndMap_ScrollRotate,
                emission2ndMap_UVMode,
                emission2ndMainStrength,
                emission2ndBlend,
                emission2ndBlendMask,
                emission2ndBlendMask_ScrollRotate,
                emission2ndBlendMode,
                emission2ndBlink,
                emission2ndUseGrad,
                emission2ndGradTex,
                emission2ndGradSpeed,
                emission2ndParallaxDepth,
                emission2ndFluorescence,

                useBumpMap,
                bumpMap,
                bumpScale,

                useBump2ndMap,
                bump2ndMap,
                bump2ndMap_UVMode,
                bump2ndScale,
                bump2ndScaleMask,

                useAnisotropy,
                anisotropyTangentMap,
                anisotropyScale,
                anisotropyScaleMask,
                anisotropyTangentWidth,
                anisotropyBitangentWidth,
                anisotropyShift,
                anisotropyShiftNoiseScale,
                anisotropySpecularStrength,
                anisotropy2ndTangentWidth,
                anisotropy2ndBitangentWidth,
                anisotropy2ndShift,
                anisotropy2ndShiftNoiseScale,
                anisotropy2ndSpecularStrength,
                anisotropyShiftNoiseMask,
                anisotropy2Reflection,
                anisotropy2MatCap,
                anisotropy2MatCap2nd,

                useBacklight,
                backlightColor,
                backlightColorTex,
                backlightMainStrength,
                backlightNormalStrength,
                backlightBorder,
                backlightBlur,
                backlightDirectivity,
                backlightViewStrength,
                backlightReceiveShadow,
                backlightBackfaceMask,

                useReflection,
                metallic,
                metallicGlossMap,
                smoothness,
                smoothnessTex,
                reflectance,
                reflectionColor,
                reflectionColorTex,
                gsaaStrength,
                applySpecular,
                applySpecularFA,
                specularNormalStrength,
                specularToon,
                specularBorder,
                specularBlur,
                applyReflection,
                reflectionNormalStrength,
                reflectionApplyTransparency,
                reflectionCubeTex,
                reflectionCubeColor,
                reflectionCubeOverride,
                reflectionCubeEnableLighting,
                reflectionBlendMode,

                useMatCap,
                matcapTex,
                matcapColor,
                matcapMainStrength,
                matcapBlendUV1,
                matcapZRotCancel,
                matcapPerspective,
                matcapVRParallaxStrength,
                matcapBlend,
                matcapBlendMask,
                matcapEnableLighting,
                matcapShadowMask,
                matcapBackfaceMask,
                matcapLod,
                matcapBlendMode,
                matcapApplyTransparency,
                matcapNormalStrength,
                matcapCustomNormal,
                matcapBumpMap,
                matcapBumpScale,

                useMatCap2nd,
                matcap2ndTex,
                matcap2ndColor,
                matcap2ndMainStrength,
                matcap2ndBlendUV1,
                matcap2ndZRotCancel,
                matcap2ndPerspective,
                matcap2ndVRParallaxStrength,
                matcap2ndBlend,
                matcap2ndBlendMask,
                matcap2ndEnableLighting,
                matcap2ndShadowMask,
                matcap2ndBackfaceMask,
                matcap2ndLod,
                matcap2ndBlendMode,
                matcap2ndApplyTransparency,
                matcap2ndNormalStrength,
                matcap2ndCustomNormal,
                matcap2ndBumpMap,
                matcap2ndBumpScale,

                useRim,
                rimColor,
                rimColorTex,
                rimMainStrength,
                rimNormalStrength,
                rimBorder,
                rimBlur,
                rimFresnelPower,
                rimEnableLighting,
                rimShadowMask,
                rimBackfaceMask,
                rimVRParallaxStrength,
                rimApplyTransparency,
                rimDirStrength,
                rimDirRange,
                rimIndirRange,
                rimIndirColor,
                rimIndirBorder,
                rimIndirBlur,
                rimBlendMode,

                useGlitter,
                glitterUVMode,
                glitterColor,
                glitterColorTex,
                glitterColorTex_UVMode,
                glitterMainStrength,
                glitterScaleRandomize,
                glitterApplyShape,
                glitterShapeTex,
                glitterAtras,
                glitterAngleRandomize,
                glitterParams1,
                glitterParams2,
                glitterPostContrast,
                glitterSensitivity,
                glitterEnableLighting,
                glitterShadowMask,
                glitterBackfaceMask,
                glitterApplyTransparency,
                glitterVRParallaxStrength,
                glitterNormalStrength,

                gemChromaticAberration,
                gemEnvContrast,
                gemEnvColor,
                gemParticleLoop,
                gemParticleColor,
                gemVRParallaxStrength,

                outlineColor,
                outlineTex,
                outlineTex_ScrollRotate,
                outlineTexHSVG,
                outlineLitColor,
                outlineLitApplyTex,
                outlineLitScale,
                outlineLitOffset,
                outlineLitShadowReceive,
                outlineWidth,
                outlineWidthMask,
                outlineFixWidth,
                outlineVertexR2Width,
                outlineDeleteMesh,
                outlineVectorTex,
                outlineVectorUVMode,
                outlineVectorScale,
                outlineEnableLighting,
                outlineZBias,
                outlineDisableInVR,

                useParallax,
                usePOM,
                parallaxMap,
                parallax,
                parallaxOffset,

                distanceFade,
                distanceFadeColor,
                distanceFadeMode,
                distanceFadeRimColor,
                distanceFadeRimFresnelPower,

                useAudioLink,
                audioLinkDefaultValue,
                audioLinkUVMode,
                audioLinkUVParams,
                audioLinkStart,
                audioLinkMask,
                audioLinkMask_ScrollRotate,
                audioLinkMask_UVMode,
                audioLink2Main2nd,
                audioLink2Main3rd,
                audioLink2Emission,
                audioLink2EmissionGrad,
                audioLink2Emission2nd,
                audioLink2Emission2ndGrad,
                audioLink2Vertex,
                audioLinkVertexUVMode,
                audioLinkVertexUVParams,
                audioLinkVertexStart,
                audioLinkVertexStrength,
                audioLinkAsLocal,
                audioLinkLocalMap,
                audioLinkLocalMapParams,

                dissolveMask,
                dissolveNoiseMask,
                dissolveNoiseMask_ScrollRotate,
                dissolveNoiseStrength,
                dissolveColor,
                dissolveParams,
                dissolvePos,

                idMaskCompile,
                idMaskFrom,
                idMaskIsBitmap,
                idMask1,
                idMask2,
                idMask3,
                idMask4,
                idMask5,
                idMask6,
                idMask7,
                idMask8,
                idMaskIndex1,
                idMaskIndex2,
                idMaskIndex3,
                idMaskIndex4,
                idMaskIndex5,
                idMaskIndex6,
                idMaskIndex7,
                idMaskIndex8,
                idMaskControlsDissolve,
                idMaskPrior1,
                idMaskPrior2,
                idMaskPrior3,
                idMaskPrior4,
                idMaskPrior5,
                idMaskPrior6,
                idMaskPrior7,
                idMaskPrior8,

                udimDiscardCompile,
                udimDiscardUV,
                udimDiscardMethod,
                udimDiscardRow3_0,
                udimDiscardRow3_1,
                udimDiscardRow3_2,
                udimDiscardRow3_3,
                udimDiscardRow2_0,
                udimDiscardRow2_1,
                udimDiscardRow2_2,
                udimDiscardRow2_3,
                udimDiscardRow1_0,
                udimDiscardRow1_1,
                udimDiscardRow1_2,
                udimDiscardRow1_3,
                udimDiscardRow0_0,
                udimDiscardRow0_1,
                udimDiscardRow0_2,
                udimDiscardRow0_3,

                ignoreEncryption,
                keys,
                bitKey0,
                bitKey1,
                bitKey2,
                bitKey3,
                bitKey4,
                bitKey5,
                bitKey6,
                bitKey7,
                bitKey8,
                bitKey9,
                bitKey10,
                bitKey11,
                bitKey12,
                bitKey13,
                bitKey14,
                bitKey15,
                bitKey16,
                bitKey17,
                bitKey18,
                bitKey19,
                bitKey20,
                bitKey21,
                bitKey22,
                bitKey23,
                bitKey24,
                bitKey25,
                bitKey26,
                bitKey27,
                bitKey28,
                bitKey29,
                bitKey30,
                bitKey31,

                refractionStrength,
                refractionFresnelPower,
                refractionColorFromMain,
                refractionColor,

                furNoiseMask,
                furMask,
                furLengthMask,
                furVectorTex,
                furVectorScale,
                furVector,
                furGravity,
                furRandomize,
                furAO,
                vertexColor2FurVector,
                furMeshType,
                furLayerNum,
                furRootOffset,
                furCutoutLength,
                furTouchStrength,
                furRimColor,
                furRimFresnelPower,
                furRimAntiLight,

                stencilRef,
                stencilReadMask,
                stencilWriteMask,
                stencilComp,
                stencilPass,
                stencilFail,
                stencilZFail,
                preStencilRef,
                preStencilReadMask,
                preStencilWriteMask,
                preStencilComp,
                preStencilPass,
                preStencilFail,
                preStencilZFail,
                outlineStencilRef,
                outlineStencilReadMask,
                outlineStencilWriteMask,
                outlineStencilComp,
                outlineStencilPass,
                outlineStencilFail,
                outlineStencilZFail,
                furStencilRef,
                furStencilReadMask,
                furStencilWriteMask,
                furStencilComp,
                furStencilPass,
                furStencilFail,
                furStencilZFail,

                subpassCutoff,
                cull,
                srcBlend,
                dstBlend,
                srcBlendAlpha,
                dstBlendAlpha,
                blendOp,
                blendOpAlpha,
                srcBlendFA,
                dstBlendFA,
                srcBlendAlphaFA,
                dstBlendAlphaFA,
                blendOpFA,
                blendOpAlphaFA,
                zclip,
                zwrite,
                ztest,
                offsetFactor,
                offsetUnits,
                colorMask,
                alphaToMask,

                preCull,
                preSrcBlend,
                preDstBlend,
                preSrcBlendAlpha,
                preDstBlendAlpha,
                preBlendOp,
                preBlendOpAlpha,
                preSrcBlendFA,
                preDstBlendFA,
                preSrcBlendAlphaFA,
                preDstBlendAlphaFA,
                preBlendOpFA,
                preBlendOpAlphaFA,
                preZclip,
                preZwrite,
                preZtest,
                preOffsetFactor,
                preOffsetUnits,
                preColorMask,
                preAlphaToMask,

                outlineCull,
                outlineSrcBlend,
                outlineDstBlend,
                outlineSrcBlendAlpha,
                outlineDstBlendAlpha,
                outlineBlendOp,
                outlineBlendOpAlpha,
                outlineSrcBlendFA,
                outlineDstBlendFA,
                outlineSrcBlendAlphaFA,
                outlineDstBlendAlphaFA,
                outlineBlendOpFA,
                outlineBlendOpAlphaFA,
                outlineZclip,
                outlineZwrite,
                outlineZtest,
                outlineOffsetFactor,
                outlineOffsetUnits,
                outlineColorMask,
                outlineAlphaToMask,

                furCull,
                furSrcBlend,
                furDstBlend,
                furSrcBlendAlpha,
                furDstBlendAlpha,
                furBlendOp,
                furBlendOpAlpha,
                furSrcBlendFA,
                furDstBlendFA,
                furSrcBlendAlphaFA,
                furDstBlendAlphaFA,
                furBlendOpFA,
                furBlendOpAlphaFA,
                furZclip,
                furZwrite,
                furZtest,
                furOffsetFactor,
                furOffsetUnits,
                furColorMask,
                furAlphaToMask,

                tessEdge,
                tessStrength,
                tessShrink,
                tessFactorMax,

                transparentModeMat,
                useClippingCanceller,
                asOverlay,
                triMask,
                matcapMul,
                fakeShadowVector
            };
        }

        private void SetProperties(MaterialProperty[] propsSource)
        {
            var allProps = AllProperties();
#if UNITY_2022_3_OR_NEWER
            var dictonary = DictionaryPool<string, MaterialProperty>.Get();
#else
            var dictonary = new Dictionary<string,MaterialProperty>();
#endif
            for (var i = 0; propsSource.Length > i; i += 1)
            {
                var p = propsSource[i];
                if (p == null) continue;
                dictonary[p.name] = p;
            }

            for (var i = 0; allProps.Length > i; i += 1)
            {
                var lilPorp = allProps[i];
                if (dictonary.TryGetValue(lilPorp.propertyName, out var materialProperty))
                    lilPorp.p = materialProperty;
            }

#if UNITY_2022_3_OR_NEWER
            DictionaryPool<string, MaterialProperty>.Release(dictonary);
#endif
        }

        private Dictionary<PropertyBlock, List<lilMaterialProperty>> GetBlock2Properties()
        {
            if (block2Propertes is null)
            {
                block2Propertes = new Dictionary<PropertyBlock, List<lilMaterialProperty>>();
                var allProps = AllProperties();

                for (var i = 0; allProps.Length > i; i += 1)
                {
                    var lilPorp = allProps[i];
                    foreach (var block in lilPorp.blocks)
                    {
                        if (block2Propertes.ContainsKey(block) is false)
                            block2Propertes[block] = new List<lilMaterialProperty>();
                        block2Propertes[block].Add(lilPorp);
                    }
                }
            }

            return block2Propertes;
        }
    }
}
#endif