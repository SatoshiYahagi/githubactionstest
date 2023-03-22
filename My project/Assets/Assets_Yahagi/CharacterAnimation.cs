using FishNet.Component.Animating;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 子オブジェクトのモデルのAnimatorを指定するので必須を外す
//[RequireComponent(typeof(Animator))]
//[RequireComponent(typeof(NetworkAnimator))]
public class CharacterAnimation : NetworkBehaviour
{
    private Animator _animator;
    private NetworkAnimator _networkAnimator;

    private void Awake()
    {
        //_animator = GetComponentInChildren<Animator>();
        //_networkAnimator = GetComponentInChildren<NetworkAnimator>();
        _animator = GetComponent<Animator>();
        _networkAnimator = GetComponent<NetworkAnimator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Jump()
    {
        _animator.SetBool("Jump", true);
        _networkAnimator.SetTrigger("Jump");
    }

    public void Grounded()
    {
        _animator.SetBool("Jump", false);
        _networkAnimator.SetTrigger("Jump");
    }
}
