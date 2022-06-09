using UnityEngine;


public class GPUGraph : MonoBehaviour {

    static readonly int
		positionsId = Shader.PropertyToID("_Positions"),
		resolutionId = Shader.PropertyToID("_Resolution"),
		stepId = Shader.PropertyToID("_Step"),
		timeId = Shader.PropertyToID("_Time");
    [SerializeField, Range(10, 200)]
    int resolution = 10;
    [SerializeField]
	ComputeShader computeShader;
    ComputeBuffer positionsBuffer;
    [SerializeField]
	Material material;

	[SerializeField]
	Mesh mesh;
    public enum TransitionMode { Cycle, Random }
    [SerializeField]
	FunctionLibrary2.FunctionName function;
	[SerializeField]
	TransitionMode transitionMode;
    [SerializeField, Min(0f)]
	float functionDuration = 1f, transitionDuration = 1f;
    float duration;
    bool transitioning;
	FunctionLibrary2.FunctionName transitionFunction;

void OnEnable(){
    // cantidad elemts del buffer como args. y tamaño exacto de cada elem. en bytes
    // necesito almacenar vector3 (3 floats de 4 bytes cada uno)
    positionsBuffer = new ComputeBuffer(resolution * resolution, 3 * 4); 
}

void OnDisable () {
    positionsBuffer.Release(); // libera memoria
    positionsBuffer = null; // esto permite recuperarlo x el proceso de recolecc basura
}

void Update()
{
    duration += Time.deltaTime;
    if (transitioning) {
        if (duration >= transitionDuration) {
            duration -= transitionDuration;
            transitioning = false;
        }
    }
    else if (duration >= functionDuration) {
        duration -= functionDuration;
        transitioning = true;
        transitionFunction = function;
        PickNextFunction();
        // function = FunctionLibrary.GetNextFunctionName(function);
    }
    UpdateFunctionOnGPU();
    // se calcula todas las posiciones del grafico en cada fotograma
}

void UpdateFunctionOnGPU () {
		float step = 2f / resolution;
		computeShader.SetInt(resolutionId, resolution);
		computeShader.SetFloat(stepId, step);
		computeShader.SetFloat(timeId, Time.time);
        computeShader.SetBuffer(0, positionsId, positionsBuffer);

        int groups = Mathf.CeilToInt(resolution / 8f); // nuestro grupo de 8 x 8 fijo , 
        // cantidad de grupos necesarios n las dimens. x e y es = resolucion/8(redondeado x arriba)
        computeShader.Dispatch(0, groups, groups, 1); // id kernel, numero de grupos
        material.SetBuffer(positionsId, positionsBuffer);
		material.SetFloat(stepId, step);
        var bounds = new Bounds(Vector3.zero, Vector3.one * (2f + 2f / resolution)); // limites espaciales

		Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, positionsBuffer.count);
	}

void PickNextFunction () {
		function = transitionMode == TransitionMode.Cycle ?
			FunctionLibrary2.GetNextFunctionName(function) :
			FunctionLibrary2.GetRandomFunctionNameOtherThan(function);
	}

}
