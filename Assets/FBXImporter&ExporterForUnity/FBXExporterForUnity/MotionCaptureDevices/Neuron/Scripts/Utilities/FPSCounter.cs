/************************************************************************************
 Copyright: Copyright 2014 Beijing Noitom Technology Ltd. All Rights reserved.
 Pending Patents: PCT/CN2014/085659 PCT/CN2014/071006

 Licensed under the Perception Neuron SDK License Beta Version (the “License");
 You may only use the Perception Neuron SDK when in compliance with the License,
 which is provided at the time of installation or download, or which
 otherwise accompanies this software in the form of either an electronic or a hard copy.

 A copy of the License is included with this package or can be obtained at:
 http://www.neuronmocap.com

 Unless required by applicable law or agreed to in writing, the Perception Neuron SDK
 distributed under the License is provided on an "AS IS" BASIS,
 WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 See the License for the specific language governing conditions and
 limitations under the License.
************************************************************************************/

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Neuron
{
	public class FPSCounter : MonoBehaviour
	{
		public enum VSyncMode
		{
			DontSync,
			EveryVBlank,
			EverySecondVBlank
		}
		
		public float updateInterval = 0.5f;
		public float FPS { get; protected set; }
		public VSyncMode mode;
		public Text textFPS = null;
		
		protected VSyncMode currentMode;
		protected float accum   = 0;
		protected int   frames  = 0;
		protected float timeleft;
		
		protected void Start()
		{
			timeleft = updateInterval;
		}
		
		protected void Update()
		{
			toggleMode();
		
			timeleft -= Time.deltaTime;
			accum += Time.timeScale / Time.deltaTime;
			++frames;
			
			if( timeleft <= 0.0f )
			{
				FPS = accum / frames;
				
				timeleft = updateInterval;
				accum = 0.0f;
				frames = 0;
			}
			
			if( textFPS != null )
			{
				textFPS.text = string.Format( "FPS: {0}", Mathf.FloorToInt( FPS ) );
			}
		}
		
		protected void toggleMode()
		{
			if( currentMode != mode )
			{
				QualitySettings.vSyncCount = (int)mode;
				currentMode = mode;
			}
			else if( (int)currentMode != QualitySettings.vSyncCount )
			{
				mode = (VSyncMode)QualitySettings.vSyncCount;
				currentMode = mode;
			}
		}
	}
}