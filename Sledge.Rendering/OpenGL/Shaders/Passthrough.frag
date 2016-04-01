#version 120

varying vec4 vertexPosition;
varying vec4 vertexNormal;
varying vec2 vertexTexture;
varying vec4 vertexMaterialColor;
varying vec4 vertexAccentColor;
varying vec4 vertexTintColor;
varying float vertexFlags;

uniform bool orthographic;
uniform bool useAccentColor;
uniform bool showGrid;
uniform float gridSpacing;
uniform sampler2D currentTexture;

int FLAGS_INVISIBLE_PERSPECTIVE = 1;
int FLAGS_INVISIBLE_ORTHOGRAPHIC = 2;
int FLAGS_SELECTED = 4;
int FLAGS_NORMALISED = 8;

bool has_flag(int value, int flag)
{
    return mod(value, flag * 2) >= flag;
}

void main()
{
    int flags = int(floor(vertexFlags));
    
    if (orthographic && has_flag(flags, FLAGS_INVISIBLE_PERSPECTIVE)) discard;
    if (!orthographic && has_flag(flags, FLAGS_INVISIBLE_ORTHOGRAPHIC)) discard;

    vec3 tint = vec3(1, 1, 1) - ((vec3(1, 1, 1) - vertexTintColor.rgb) * vertexTintColor.a);

    vec4 fragmentColor = useAccentColor ? vertexAccentColor : texture2D(currentTexture, vertexTexture) * vertexMaterialColor;
	if (!useAccentColor) fragmentColor.rgb *= tint; // vertexTintColor.rgb * vertexTintColor.a;
	fragmentColor.a *= vertexMaterialColor.a;

    if (fragmentColor.a < 0.05) discard;
	
    if (showGrid) {
        if (abs(vertexNormal).x < 0.9999) fragmentColor = mix(fragmentColor, vec4(1, 0, 0, 1), step(mod(vertexPosition.x, gridSpacing), 0.1f));
        if (abs(vertexNormal).y < 0.9999) fragmentColor = mix(fragmentColor, vec4(0, 1, 0, 1), step(mod(vertexPosition.y, gridSpacing), 0.1f));
        if (abs(vertexNormal).z < 0.9999) fragmentColor = mix(fragmentColor, vec4(0, 0, 1, 1), step(mod(vertexPosition.z, gridSpacing), 0.1f));
    }
    
    gl_FragColor = fragmentColor;
}
