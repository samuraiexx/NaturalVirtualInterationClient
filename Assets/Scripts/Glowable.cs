using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Glowable : MonoBehaviour {

    private enum State {
        NotGlowing,
        Glowing
    }

    State state;

    public Material defaultMaterial;
    public Material glowingMaterial;

    // Start is called before the first frame update
    void Start() {
        state = State.NotGlowing;
    }

    // Update is called once per frame
    void Update() {}

    public void Glow() {
        if (state == State.NotGlowing) {
            gameObject.GetComponent<MeshRenderer>().material = glowingMaterial;
            state = State.Glowing;
        }
    }

    public void Unglow() {
        if (state == State.Glowing) {
            gameObject.GetComponent<MeshRenderer>().material = defaultMaterial;
            state = State.NotGlowing;
        }
    }
}
