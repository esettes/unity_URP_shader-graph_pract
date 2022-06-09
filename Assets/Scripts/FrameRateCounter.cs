using UnityEngine;
using TMPro;

public class FrameRateCounter : MonoBehaviour {

	[SerializeField]
	TextMeshProUGUI display;
    public enum DisplayMode { FPS, MS }

	[SerializeField]
	DisplayMode displayMode = DisplayMode.FPS;
    [SerializeField, Range(0.1f, 2f)]
	float sampleDuration = 1f;
    int frames;
	float duration, bestDuration = float.MaxValue, worstDuration;
    void Update()
    {
        //  Para mostrar la velocidad de fotogramas expresada como fotogramas por segundo,
        // tenemos que mostrar su inverso, por lo que uno dividido por la duración del fotograma.
        //  cuántos fotogramas se han renderizado y la duración total, 
        // y luego mostramos la cantidad de fotogramas dividida por su duración combinada.
        float frameDuration = Time.unscaledDeltaTime;
        frames += 1;
		duration += frameDuration;

        if (frameDuration < bestDuration) {
			bestDuration = frameDuration;
		}
		if (frameDuration > worstDuration) {
			worstDuration = frameDuration;
		}

		if (duration >= sampleDuration) {
			if (displayMode == DisplayMode.FPS) {
				display.SetText(
					"FPS\n{0:0}\n{1:0}\n{2:0}",
					1f / bestDuration,
					frames / duration,
					1f / worstDuration
				);
			}
			else {
				display.SetText(
					"MS\n{0:1}\n{1:1}\n{2:1}",
					1000f * bestDuration,
					1000f * duration / frames,
					1000f * worstDuration
				);
			}
			frames = 0;
			duration = 0f;
			bestDuration = float.MaxValue;
			worstDuration = 0f;
		}
    }
}