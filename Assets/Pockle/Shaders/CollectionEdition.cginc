#ifndef POCKLE_COLLECTION_EDITION
#define POCKLE_COLLECTION_EDITION
// Local rarity layer; face ink and accessories are excluded by the caller's pigment mask.
half _EditionEffect, _RosterPalette; float _EditionClock;
half4 _EditionColor;
float EditionHash(float3 p) { return frac(sin(dot(p,float3(127.1,311.7,74.7)))*43758.5453); }
half3 CollectionEdition(half3 original,float3 p,half3 n,half3 view,half mask,half3 pigment)
{
    if (_EditionEffect < .5) return original;
    half3 ray=reflect(-view,n),key=normalize(half3(.64,.70,-.66));
    half rim=1-saturate(dot(n,view));
    half3 env=lerp(texCUBE(_StudioCube,ray).rgb,texCUBE(_StageEnvironment,ray).rgb,_StageEnvironmentStrength);
    half3 color=original;
    if (_EditionEffect < 2.5)
    {
        half panel=exp2(-pow(ray.x+.25,2)*20-pow(ray.y-.35,2)*10);
        half strip=pow(saturate(1-abs(ray.x-.65)*5),3);
        half3 f0=lerp(pigment,half3(1,.98,.92),.18);
        color=pigment*.22+env*f0*1.25+f0*(.3+panel*1.05+strip*.65+rim*.12);
        if (_EditionEffect > 1.5)
        {
            float leaf=EditionHash(floor(p*32));
            color*=.78+.22*smoothstep(.35,.52,leaf);
            color+=pigment*smoothstep(.75,.95,leaf)*.16;
        }
    }
    else if (_EditionEffect < 3.5)
    {
        float2 uv=p.xy*4.6+p.z*float2(1.3,.9);uv.x+=floor(uv.y)*.35;
        float2 cell=floor(uv),f=frac(uv);
        float halfFacet=EditionHash(float3(cell,8))>.5?step(f.x,f.y):step(1,f.x+f.y);
        float3 tile=float3(cell,halfFacet);
        half3 cut=half3(EditionHash(tile)-.5,EditionHash(tile+7)-.5,EditionHash(tile+13)-.5);
        half3 facetNormal=normalize(n+UnityObjectToWorldNormal(cut)*1.7);
        half3 facetRay=reflect(-view,facetNormal);
        half light=saturate(dot(facetNormal,key)),t=EditionHash(tile+31);
        half3 prism=.5+.5*cos(half3(0,2.1,4.2)+t*6.28+dot(facetRay,half3(1,2,3))*3);
        half3 reflected=lerp(texCUBE(_StudioCube,facetRay).rgb,texCUBE(_StageEnvironment,facetRay).rgb,_StageEnvironmentStrength);
        color=lerp(half3(.045,.16,.34),half3(.73,.89,1),light*.65+t*.35);
        color+=reflected*.65+prism*pow(1-saturate(dot(facetNormal,view)),3)*.3;
        color+=half3(.9,.97,1)*(pow(saturate(dot(facetRay,key)),70)*1.3+pow(saturate(dot(facetRay,key)),12)*.35);
    }
    else
    {
        half pulse=.88+.12*sin(_EditionClock*1.3);
        half3 glow=_EditionColor.rgb;
        // Cosmic Core / Polar Night / Sugar Nebula: deep glass with a star field and nebular bands.
        if (_EditionEffect < 4.5)
        {
            float3 localView=normalize(mul((float3x3)unity_WorldToObject,view));
            float3 inside=p-localView*.16;
            half cloud=.5+.5*sin(inside.y*7+sin(inside.x*6+inside.z*2)*1.8);
            half stars=smoothstep(.985,.999,EditionHash(floor(inside*90)));
            color=half3(.09,.13,.23)+glow*pow(cloud,3)*.38+half3(.30,.08,.47)*pow(1-cloud,3)*.45;
            color+=env*.3+glow*(stars*(1.1+.3*pulse)+pow(rim,3)*.28);
        }
        // Aurora Evergreen / Sunfire Spirit / Solstice Spirit: flowing luminous ribbons.
        else if (_EditionEffect < 5.5)
        {
            half ribbons=pow(saturate(1-abs(sin(p.y*6+p.x*2+sin(p.x*4+p.z*2+_EditionClock*.15)*1.5))),7);
            half3 spectrum=.5+.5*cos(half3(0,2.2,4.2)+p.y*2+dot(view,n)*3);
            color=glow*.08+env*.3+lerp(glow,spectrum,.45)*ribbons*(.75+.25*pulse)+glow*pow(rim,3)*.22;
        }
        // Biolume Bloom: luminous organic spots over dark lagoon glass.
        else if (_EditionEffect < 6.5)
        {
            float3 cell=floor(p*16),f=frac(p*16)-.5;
            half spot=(1-smoothstep(.07,.17,dot(f,f)))*step(.45,EditionHash(cell));
            color=half3(.025,.12,.17)+env*.28+glow*(spot*(.85+.25*pulse)+pow(rim,3)*.22);
        }
        // Midnight Ganache: gold marbling over reflective dark chocolate.
        else if (_EditionEffect < 7.5)
        {
            half swirl=pow(saturate(1-abs(sin(p.y*9+p.x*3+sin(p.x*5+p.z*3)*2))),9);
            color=half3(.095,.035,.055)+env*.3+glow*swirl*.85;
        }
        // Polar Opal: milk-white iridescent islands, rather than dark molten rock.
        else if (_EditionEffect < 8.5)
        {
            half3 opal=.5+.5*cos(half3(0,2,4)+p.y*7+sin(p.x*9+p.z*4)*2+dot(n,view)*4);
            color=half3(.65,.79,.86)+opal*.22+env*.27+glow*pow(rim,3)*.12;
        }
        // Stained Sugar: broad translucent-looking jewel cells with bright sugar seams.
        else
        {
            float2 cell=floor(p.xy*8+p.z*2),f=frac(p.xy*8+p.z*2);
            half seed=EditionHash(float3(cell,6));
            half seam=1-smoothstep(.01,.035,min(min(f.x,1-f.x),min(f.y,1-f.y)));
            color=(.5+.5*cos(half3(0,2.1,4.2)+seed*6.28))*.65+env*.38+seam*glow*.7;
        }
    }
    return lerp(original,color,saturate(mask));
}
#endif
