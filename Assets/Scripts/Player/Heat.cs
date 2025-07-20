using System.Collections.Generic;
using UnityEngine;

public class Heat : MonoBehaviour
{
    Player player;
    Rigidbody rb;

    public float bodyTemp = 37.0f;
    public float coldDeathTemp = 28f;
    public float heatDeathTemp = 43f;
    public float defaultHPS = 0.05f;

    private List<TempChange> activeTempChanges = new();

    private void Awake()
    {
        player = GetComponent<Player>();
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (!player.isAlive || GameManager.Instance.isCutscene) return;

        float deltaTempFromChanges = 0f;

        for (int i = activeTempChanges.Count - 1; i >= 0; i--)
        {
            if (activeTempChanges[i].Update(Time.deltaTime, out float delta))
            {
                deltaTempFromChanges += delta;
            }
            else
            {
                deltaTempFromChanges += delta;
                activeTempChanges.RemoveAt(i);
            }
        }

        float baseTempGain = defaultHPS * Time.deltaTime;

        bodyTemp += baseTempGain + deltaTempFromChanges;

        CheckBodyTemp();
    }


    private void CheckBodyTemp()
    {
        if (!player.isAlive) return;
        if (bodyTemp > heatDeathTemp || bodyTemp < coldDeathTemp)
        {
            player.Die(bodyTemp > heatDeathTemp);
        }
    }

    public void ChangeTemp(float tempChange, float duration = 1f)
    {
        activeTempChanges.Add(new TempChange(tempChange, duration));
    }
}

class TempChange
{
    private float totalChange;
    private float duration;
    private float elapsed;

    public TempChange(float totalChange, float duration)
    {
        this.totalChange = totalChange;
        this.duration = duration;
        elapsed = 0f;
    }

    public bool Update(float deltaTime, out float delta)
    {
        float prevElapsed = elapsed;
        elapsed += deltaTime;

        if (elapsed >= duration)
        {
            float prevRatio = Mathf.Clamp01(prevElapsed / duration);
            delta = totalChange * (1f - prevRatio);
            return false;
        }
        else
        {
            float prevRatio = Mathf.Clamp01(prevElapsed / duration);
            float currRatio = Mathf.Clamp01(elapsed / duration);
            delta = totalChange * (currRatio - prevRatio);
            return true;
        }
    }
}
