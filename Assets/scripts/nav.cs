using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class nav : MonoBehaviour {


	// Use this for initialization

	public GameObject text;//天数的文字UI
	public GameObject target;//教室目标寻路地点的空物体
	public GameObject diningtarget;//食堂目标寻路地点的空物体
	public GameObject geliTarget;//隔离区目标寻路地点的空物体
	//其实Scene里放的这几个空物体应该是没用的 目标点坐标都取决于Assets里的prefab的坐标 要改目的地的话prefab一定要改

	private NavMeshAgent agent;
	private Vector3 startPoint;//出生点 也就是宿舍的坑位

	public int infected = 0;//0健康 1潜伏 2发病 3潜伏可传染
	public int isGeli;//点击studet的prefab 在inspector界面手动改动 是否隔离 0不会自我隔离 1会在发病后自动去医院

	private int emergeGeli = 0;//紧急状态 如果开启马上回宿舍并告诉所有碰到的人


	Color green = new Color (34f / 255f, 229f / 255f, 132f / 255f);//没用上
	Color blue = new Color (32f / 255f, 144f / 255f, 255f / 255f);//没用上
	Color orange = new Color (255f / 255f, 112f / 255f, 0f / 255f);

	private Text mText;

	void Start () {
		agent = GetComponent<NavMeshAgent>();//地形改动以后记得加static以及Navigation重新bake
		mText = text.GetComponent <Text> ();
		startPoint = this.gameObject.transform.position;//记录出生复活点位置

	}
	private int state = 0;

	private int count = 6 * 60;//计数 按60fps算 就是6秒

	private int day = 0;//第几天
	// Update is called once per frame
	void FixedUpdate () {//注意是Fixed
		
		if(cover + infecDay - 2 <= day)//天亮了 如果潜伏可传染期到达 - 2就是前两天有传染性
			infected = 3;//橙色
		if((cover + infecDay) <= day)//如果发病期到达
			infected = 1;//变红
		
		
		if(infected == 1)//如果带毒 则变红
			this.gameObject.GetComponent<Renderer> ().material.color = Color.red;
		else if(infected == 2)//如果潜伏 则变黄(?)
			this.gameObject.GetComponent<Renderer> ().material.color = Color.yellow;
		else if(infected == 3)//如果能感染的潜伏 则变橙色
			this.gameObject.GetComponent<Renderer> ().material.color = orange;	


		count++;

		if(count > 6 * 60)//一看就是写单片机的 每6*60个计数进这个函数一次
		{
			count = 0;

			if (state == 0)//去教室
			{
				day++;
				mText.text = day.ToString();
				agent.enabled = true;
				state = 1;
				agent.destination = target.transform.position;
			}
			else if(state == 1)//去食堂
			{
				agent.enabled = true;
				state = 2;
				agent.destination = diningtarget.transform.position;
			}
			else if(state == 2)//回宿舍
			{
				agent.enabled = true;
				state = 0;
				agent.destination = startPoint;
			}
				


		}
		if (infected == 1 && isGeli == 1)
		{//如果开启自我隔离并且发病
			agent.destination = geliTarget.transform.position;
			SendMessage ("geli");//这个应该是没啥用 怕出问题 不删了
		} 

		if(infected != 1 && emergeGeli == 1)//如果没发病并且进入紧急状态
			agent.destination = startPoint;//回宿舍
 
	}
		

	//infecRate = 0.1;
	private void OnCollisionEnter(Collision collision)
	{
		if (collision.collider.tag == "stu")//如果碰上了其他学生
		{
			int i = Random.Range (0, 1000);
			if (i <= 50 && infected == 1)//发病期这个传染率
				collision.gameObject.SendMessage ("inf");//告诉他 你被传染了！
			if (i <= 20 && infected == 3)//潜伏期这个传染率
				collision.gameObject.SendMessage ("inf");
			//Destroy (collision.collider.gameObject);
			if(infected == 1 || emergeGeli == 1)//如果我发病了 或我知道有人发病了 我就告诉被碰的人 进入紧急状态
				collision.gameObject.SendMessage ("geli");
		}
	}
	private int cover = 999;//潜伏期 由于前面已经开始判断是否发病 只能用很大的数防止开局全体感染
	private int infecDay = 999;//被感染日期
	void inf()
	{
		if (infected == 0) {//如果没被感染
			infecDay = day;
			cover = Random.Range (3, 7);//算出了自己的潜伏期 这里应该用正态分布比较好 但是C#好像没有Python那种一行代码搞定的正态分布
			infected = 2;//进入潜伏期
		}
	}

	void geli()
	{
		emergeGeli = 1;//当他为1进入紧急状态 马上回宿舍瑟瑟发抖
	}
}
