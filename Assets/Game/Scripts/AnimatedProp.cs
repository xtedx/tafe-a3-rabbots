using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedProp : MonoBehaviour
{
    private Animator _animator;

    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    public void Animate()
    {
        if (name.StartsWith("Mattress"))
        {
            Debug.Log($"in animatedprop animate {name}");
            _animator.Play("MattressBounce");
        }
        else if (name.StartsWith("Tire"))
        {
            Debug.Log($"in animatedprop animate2 {name}");
            _animator.Play("TireBounce");
        }
    }
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Debug.Log($"in animatedprop OnControllerColliderHit {hit.gameObject.name}");
        Animate();
    }

    private void OnCollisionEnter(Collision other)
    {
        Debug.Log($"in animatedprop OnCollisionEnter {other.gameObject.name}");
        Animate();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"in animatedprop OnTriggerEnter {other.gameObject.name}");
        Animate();
    }
}