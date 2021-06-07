﻿using Vitrivr.UnityInterface.CineastApi.Model.Data;
using UnityEngine;

namespace VitrivrVR.Query.Display
{
  /// <summary>
  /// Abstract class for displaying queries.
  /// </summary>
  public abstract class QueryDisplay : MonoBehaviour
  {
    public virtual int NumberOfResults => -1;

    public abstract void Initialize(QueryResponse queryData);
  }
}