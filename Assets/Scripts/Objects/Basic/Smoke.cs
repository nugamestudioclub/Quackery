using UnityEngine;

public class Smoke : MonoBehaviour
{
    private float aliveFor = 0f;
    private Renderer rend;
    private MaterialPropertyBlock mblock;

    [SerializeField] private float lifeTime = 0.5f;
    [SerializeField] private float startSize = 0.5f;
    [SerializeField] private float endSize = 2f;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        mblock = new MaterialPropertyBlock();

        // Divide by zero safety. lifeTime should never be 0 anyways though.
        if (lifeTime == 0f) {
            lifeTime = 0.001f;
        }
    }

    public void setProps(float lifeTime_, float startSize_, float endSize_) {
        lifeTime = lifeTime_;
        startSize = startSize_;
        endSize = endSize_;
    }

    private void Update()
    {
        if (aliveFor >= lifeTime) {
            Object.Destroy(gameObject);
        }

        float desiredAlpha = (lifeTime - aliveFor) / lifeTime;
        float desiredScale = startSize + ((endSize - startSize) * (aliveFor / lifeTime));

        rend.GetPropertyBlock(mblock);
        Color color = mblock.GetColor("_BaseColor");
        color.a = desiredAlpha;
        mblock.SetColor("_BaseColor", color);
        rend.SetPropertyBlock(mblock);

        transform.localScale = new Vector3(desiredScale, desiredScale, desiredScale);

        aliveFor += Time.deltaTime;
    }
}
