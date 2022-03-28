using UnityEngine.Events;

// easily store the data of our events
// similarly to C unions: we define several fields but we use only one at a time
public class CustomEventData
{
    public UnitData unitData;

    public CustomEventData(BuildingData unitData)
    {
        this.unitData = unitData;
    }
}

// custom type of event that contain data
[System.Serializable]
public class CustomEvent : UnityEvent<CustomEventData> {}