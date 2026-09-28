// Shapes 50-59 of the FxProc shader. Owned by one worker; add `else if (mode < N)` branches. Available: hash, vnoise, fbm, u (-1..1 quad
// coords), q (progress 0..1), seed, par (free parameter), r (length of u). Set I (intensity 0..1) and optionally hot (the colour I turns
// towards when bright, default white). The quad's tint colour and alpha are applied by the caller.
#ifndef FX_SHAPES_D
#define FX_SHAPES_D
void ShapeD(float mode, float2 u, float q, float seed, float par, float r, inout float I, inout float3 hot)
{
    I = 0;
}
#endif
