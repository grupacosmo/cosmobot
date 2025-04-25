using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Jint;
using System.Reflection;
using System.Collections.Concurrent;

namespace Cosmobot
{
    public class TestingThread : MonoBehaviour
    {
        Task task;
        TestingCoroutine a;

        void Start()
        {
            a = gameObject.GetComponent<TestingCoroutine>();
            task = Task.Run(() => Thead());
        }

        void Thead()
        {
            for(int i = 0; i<10;i++)
            { 
                a.CQ.Enqueue(() => {transform.position += transform.forward * 5 * Time.deltaTime;});
                a.CQ.Enqueue(() => {transform.position += transform.forward * 5 * Time.deltaTime;});
                a.CQ.Enqueue(() => {transform.position += transform.forward * 5 * Time.deltaTime;});
                a.CQ.Enqueue(() => {transform.position += transform.forward * 5 * Time.deltaTime;});
                a.CQ.Enqueue(() => {transform.position += transform.forward * 5 * Time.deltaTime;});
                a.CQ.Enqueue(() => {transform.position += transform.forward * 5 * Time.deltaTime;});
                a.CQ.Enqueue(() => {transform.position += transform.forward * 5 * Time.deltaTime;});
                a.CQ.Enqueue(() => {transform.position += transform.forward * 5 * Time.deltaTime;});
                a.CQ.Enqueue(() => {transform.position += transform.forward * 5 * Time.deltaTime;});
                a.CQ.Enqueue(() => {transform.position += transform.forward * 5 * Time.deltaTime;});
                a.CQ.Enqueue(() => {transform.position += transform.forward * 5 * Time.deltaTime;});
                a.CQ.Enqueue(() => {transform.position += transform.forward * 5 * Time.deltaTime;});
                a.CQ.Enqueue(() => {transform.position += transform.forward * 5 * Time.deltaTime;});
                a.CQ.Enqueue(() => {transform.position += transform.forward * 5 * Time.deltaTime;});
                a.CQ.Enqueue(() => {transform.position += transform.forward * 5 * Time.deltaTime;});
                a.CQ.Enqueue(() => {transform.position += transform.forward * 5 * Time.deltaTime;});
                a.CQ.Enqueue(() => {transform.position += transform.forward * 5 * Time.deltaTime;});
                a.CQ.Enqueue(() => {transform.position += transform.forward * 5 * Time.deltaTime;});
            }
        }
    }
}
