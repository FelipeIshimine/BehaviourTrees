using System.Collections.Generic;
using UnityEngine;

public interface IGetNavAgentTargets
{
    public IReadOnlyList<Transform> Get();
}