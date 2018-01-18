using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Structs : MonoBehaviour {

    [Serializable]
    public class DataCollection
    {
        [SerializeField]
        public List<ListWrapper> Hand;
        [SerializeField]
        public List<Vector2> FriendlyBoard;
        [SerializeField]
        public List<Vector2> EnemyBoard;
        [SerializeField]
        public List<Vector2> extra;
    }

    [System.Serializable]
    public class ListWrapper
    {
        public List<Vector2> myList;
    }
}
