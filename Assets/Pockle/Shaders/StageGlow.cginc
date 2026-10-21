#ifndef POCKLE_STAGE_GLOW_INCLUDED
#define POCKLE_STAGE_GLOW_INCLUDED
// Self-lit pigment with a soft view-dependent rim. No bloom, scene grabs or lighting passes.
half3 PockleStageGlow(half3 pigment, half strength, half surfaceMask, half facing)
{
    half rim = 1 - saturate(facing);
    half3 luminous = lerp(pigment, max(pigment, half3(.7,.7,.7)), rim * .2h);
    return luminous * strength * saturate(surfaceMask) * (.9h + 1.15h * rim * rim);
}
#endif
