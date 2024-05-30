using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace SwapChains.Runtime.VFX
{
    public class LightCookieMotion : MonoBehaviour
    {
        [Header("Texture 1")]
        [SerializeField] Vector2 m_cycleDuration1UV = new(20f, 20f);
        [SerializeField] AnimationCurve m_movementPath1U;
        [SerializeField] AnimationCurve m_movementPath1V;
        [SerializeField] Vector2 m_movementMagnitude1UV = new(0.1f, 0.1f);
        [SerializeField] Vector2 m_movementTimeOffset1UV = new();
        [SerializeField] Vector2 m_tex1TilingUV = new(1f, 1f);
        [SerializeField] Vector2 m_tex1OffsetUV = new();
        [Header("Texture 2")]
        [SerializeField] Vector2 m_cycleDuration2UV = new(20f, 20f);
        [SerializeField] AnimationCurve m_movementPath2U;
        [SerializeField] AnimationCurve m_movementPath2V;
        [SerializeField] Vector2 m_movementMagnitude2UV = new(0.1f, 0.1f);
        [SerializeField] Vector2 m_movementTimeOffset2UV = new();
        [SerializeField] Vector2 m_tex2TilingUV = new(2f, 2f);
        [SerializeField] Vector2 m_tex2OffsetUV = new();
        [Header("Refs")]
        [SerializeField] UniversalAdditionalLightData lightData;
        float timer;

        void OnValidate() => UpdateMaterial();

        void Update()
        {
            timer = Time.time;
            UpdateMaterial();
        }

        void UpdateMaterial()
        {
            lightData.lightCookieOffset = UpdateCookieMovement(
                m_cycleDuration1UV, m_movementPath1U, m_movementPath1V, m_movementTimeOffset1UV, m_movementMagnitude1UV, m_tex1TilingUV, m_tex1OffsetUV);
            lightData.lightCookieOffset = UpdateCookieMovement(
                m_cycleDuration2UV, m_movementPath2U, m_movementPath2V, m_movementTimeOffset2UV, m_movementMagnitude2UV, m_tex2TilingUV, m_tex2OffsetUV);
        }

        Vector4 UpdateCookieMovement(
            Vector2 m_cycleDurationUV, AnimationCurve m_movementPathU, AnimationCurve m_movementPathV, Vector2 m_movementTimeOffsetUV,
            Vector2 m_movementMagnitudeUV, Vector2 m_texTilingUV, Vector2 m_texOffsetUV)
        {
            float m_timeU;
            m_timeU = timer % m_cycleDurationUV.x;
            m_timeU /= m_cycleDurationUV.x;

            float m_timeV;
            m_timeV = timer % m_cycleDurationUV.y;
            m_timeV /= m_cycleDurationUV.y;

            var newU = m_movementPathU.Evaluate(m_timeU + m_movementTimeOffsetUV.x) * m_movementMagnitudeUV.x;
            var newV = m_movementPathV.Evaluate(m_timeV + m_movementTimeOffsetUV.y) * m_movementMagnitudeUV.y;

            Vector4 newUV;
            newUV.x = m_texTilingUV.x;
            newUV.y = m_texTilingUV.y;
            newUV.z = newU + m_texOffsetUV.x;
            newUV.w = newV + m_texOffsetUV.y;

            return newUV;
        }
    }
}