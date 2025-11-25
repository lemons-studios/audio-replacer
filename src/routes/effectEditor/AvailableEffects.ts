// This is a WORK-IN-PROGRESS and probably won't be finished for 5.0 (maybe a hypothetical 5.1 if I'm feeling up to it)
export const availableEffects = [
    {
        name: "Compressor",
        description: "Compress the audio",
        parameters: [
            {
                name: "threshold",
                description: "Volume level when audio is compressed",
                min: 0.00097563,
                max: 1,
                defaultValue: 0.125
            },
            {
                name: "ratio",
                description: "Ratio by which the audio is compressed",
                min: 1,
                max: 20,
                defaultValue: 2,
            },
            {
                name: "attack",
                description: "Amount of time the signal has to rise above the threshold before gain reduction starts (in milliseconds)",
                min: 0.01,
                max: 2000,
                defaultValue: 20,
            },
            {
                name: "release",
                description: "Amount of time the signal has to fall below the threshold before reduction is decreased again (in milliseconds)",
                min: 0.01,
                max: 9000,
                defaultValue: 250
            }
        ],
        generateEffectStr: (t: number = 0.125, ra: number = 2, a: number = 20, re: number = 250): string => {
            return `acompressor=threshold=${clamp(t, 0.00097563, 1)}dB:ratio=${clamp(ra, 1, 20)}:attack=${clamp(a, 0.01, 2000)}:release=${clamp(re, 0.01, 9000)}`;
        }
    },
    {
        name: "Contrast",
        description: "Simple audio dynamic range compression/expansion filter",
        parameters: [
            {
                name: "contrast",
                description: "Set contrast",
                min: 0,
                max: 100,
                defaultValue: 33
            }
        ],
        generateEffectStr: (c: number) => {
            return `acontrast=contrast=${clamp(c, 0, 100)}`;
        }
    },
    {
        name: "Crusher",
        description: "Reduce audio quality",
        parameters: [

        ]
    }
] as const

const clamp = (x: number, min: number, max: number) => {
    return Math.min(Math.max(x, min), max);
}

export const availableAdvancedEffects: string[] = [""];