using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using System.Collections;

public class GoalInfo : MonoBehaviour
{
    public AnimationCurve curve;
    public AnimationCurve curveExit;
    public Goal goal;
    public Stats globalStats;
    public TMP_Text nameDisplay;
    public TMP_Text amountDisplay;
    public Image fluidDisplay;
    public float moveSpeed = 5f;

    private float fluidAtStart = 0f;
    private bool ready = false;
    private float t = 0f;
    private Vector3 targetPos;
    public float targetY;
    private void Update()
    {
        
        t = Mathf.MoveTowards(t, 1, moveSpeed);
        transform.position = Vector3.Lerp(transform.position, targetPos, curve.Evaluate(t));
        
        if (!ready) {return;}
        
        fluidDisplay.color = goal.reqFluid.color;
        
        nameDisplay.text = goal.reqFluid.name;
        if (globalStats.heldFluids.TryGetValue(goal.reqFluid.identifier, out float storedAmount))
            amountDisplay.text = (Mathf.Round((storedAmount - fluidAtStart) * 100f) / 100f).ToString() + "L / " + (Mathf.Round(goal.reqAmount * 100f) / 100f).ToString() + "L";
        else
            amountDisplay.text = "0L / " + goal.reqAmount.ToString() + "L";
        
        if (globalStats.heldFluids.TryGetValue(goal.reqFluid.identifier, out float stored))
        {
            if (stored - fluidAtStart >= goal.reqAmount)
            {
                goal.completed = true;
            }
        }
    }

    private void Start()
    {
        targetPos = new Vector3(transform.position.x + 600f, targetY, 0f);
        if (globalStats == null)
        {
            globalStats = FindAnyObjectByType<Stats>();
        }
        
        if (globalStats.heldFluids.TryGetValue(goal.reqFluid.identifier, out float storedAmount))
            fluidAtStart = storedAmount;
        
        ready = true;

    }

    public IEnumerator Exit() // fix this.
    {
        t = 0f;
        while (t < 1)
        {
            t = Mathf.MoveTowards(t, 1, moveSpeed);
            transform.position = Vector3.Lerp(transform.position, new Vector3(targetPos.x - 600f, transform.position.y, 0f), curveExit.Evaluate(t));
            yield return new WaitForEndOfFrame();
        }
        Destroy(gameObject);
    }
}
