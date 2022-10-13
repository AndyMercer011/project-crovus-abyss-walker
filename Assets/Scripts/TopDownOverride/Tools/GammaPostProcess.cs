using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Rendering.Universal;

public class GammaPostProcess : MonoBehaviour
{
    public float gamma;

    private void OnValidate()
    {
#if Unity_URP
        Volume urpVolume = gameObject.GetComponent<Volume>();
        if (urpVolume != null)
        {
            LiftGammaGain liftGammaGain;
            urpVolume.profile.TryGet(out liftGammaGain);
            Vector4 last = liftGammaGain.gamma.value;
            liftGammaGain.gamma.value = new Vector4(last.x, last.y, last.z, gamma);
        }
#else
        PostProcessVolume volume;
        ColorGrading colorGrading;
        volume = gameObject.GetComponent<PostProcessVolume>();
        if (volume != null)
        {
            volume.profile.TryGetSettings(out colorGrading);
            Vector4 last = colorGrading.gamma.value;
            colorGrading.gamma.value = new Vector4(last.x, last.y, last.z, gamma);
        }
#endif
    }
}
