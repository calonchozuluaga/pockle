#ifndef POCKLE_FUSION_PATTERN
#define POCKLE_FUSION_PATTERN
half _FusionPattern;
half4 _FusionPigmentA, _FusionPigmentB, _FusionSurfaceA, _FusionSurfaceB;
half4 _FusionDetailA, _FusionDetailB, _FusionAccentA, _FusionAccentB;
// All samples use immutable mesh coordinates: markings travel with the toy.
half FusionMix(float3 p)
{
    if (_FusionPattern < .5) return 0;
    if (_FusionPattern < 1.5) return smoothstep(-.48,.48,p.y+p.x*.35);
    float wave=sin(p.y*8+p.x*5+sin(p.z*5+p.x*3)*1.6);
    if (_FusionPattern < 2.5) return smoothstep(-.24,.24,wave);
    if (_FusionPattern < 3.5)
    {
        float3 q=p*4.5+float3(sin(p.y*3),sin(p.z*3),sin(p.x*3))*.3;
        float3 cell=frac(q)-.5;
        return 1-smoothstep(.26,.34,length(cell));
    }
    return 1-smoothstep(.20,.38,abs(wave));
}
half3 FusionPigment(float3 p, half3 original)
{
    return _FusionPattern>.5 ? lerp(_FusionPigmentA.rgb,_FusionPigmentB.rgb,FusionMix(p)) : original;
}
// x gloss (Pip: roughness), y metallic, z fuzz, w opacity.
half4 FusionSurface(float3 p, half4 original)
{
    return _FusionPattern>.5 ? lerp(_FusionSurfaceA,_FusionSurfaceB,FusionMix(p)) : original;
}
half4 FusionDetail(float3 p, half4 original)
{ return _FusionPattern>.5 ? lerp(_FusionDetailA,_FusionDetailB,FusionMix(p)) : original; }
half4 FusionAccent(float3 p, half4 original)
{ return _FusionPattern>.5 ? lerp(_FusionAccentA,_FusionAccentB,FusionMix(p)) : original; }
#endif
