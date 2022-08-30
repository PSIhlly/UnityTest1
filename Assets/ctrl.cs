using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class ctrl : MonoBehaviour
{
    // Start is called before the first frame update
 public UnityEngine.UI.Image img;
 public Texture2D pic;
 void Start()
 {
  print(1);
  a();
  print(2);
 }

 public async void a()
 {
     await b();
     print(3);
 }

 async public Task b()
 {
     await Task.Run(() =>
     {
         for (int i = 1; i <= 1000000; i++) i++;
         print(5);
     });
     c();
     print(4);
 }

 public void c()
 {
     print(6);
 }

}
