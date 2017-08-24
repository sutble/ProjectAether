using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct SpellStats
{
    public int id;
    public float startTime;
    public int cooldown;
    public int castDuration;
}

public class SpellController : MonoBehaviour {

    Dictionary<int, SpellStats> cooldownTable;
    Dictionary<int, float> lastGestures;

    public ControllerInterface CI;

    // Use this for initialization
    protected void Start () {
        CI = GetComponent<ControllerInterface>();
        cooldownTable = new Dictionary<int, SpellStats>();
        lastGestures = new Dictionary<int, float>();

        CI.gestureEvents.UpwardGesture += new GestureEventHandler(trackUpwardGesture);
    }

    void trackUpwardGesture(object sender, GestureEventArgs e)
    {
        lastGestures[0] = Time.time;
    }

    protected float getLastUpwardGesture()
    {
        return lastGestures.ContainsKey(0) ? lastGestures[0] : 0.0F;
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public int GetDeviceIndex()
    {
        return (int)this.gameObject.GetComponent<SteamVR_TrackedObject>().index;
    }

    public void SetValues(int id, float startTime, int cd, int dur)
    {
        SpellStats temp = new SpellStats();
        if (cooldownTable.ContainsKey(id))
        {
            temp = cooldownTable[id];
        }
        else
        {
            temp.id = id;
            cooldownTable.Add(id, temp);
        }
        temp.startTime = startTime;
        temp.cooldown = cd;
        temp.castDuration = dur;

    }

    public void castSpell(int id, int cd, int dur)
    {
        SetValues(id, Time.time, cd, dur);
    }

    public void cancelSpell(int id)
    {
        if (isSpellCasted(id))
        {
            // no change to cooldown
        }
        else
        {
            // reset cooldown
            SetValues(id, 0, -1, -1);
        }
    }

    // returns true if is passed the spell cooldown time
    public bool isSpellCooldown(int id)
    {
        return cooldownTable[id].cooldown > Time.time - cooldownTable[id].startTime;
    }

    public bool isSpellCasted(int id)
    {
        return cooldownTable[id].castDuration > Time.time - cooldownTable[id].startTime;
    }


}
