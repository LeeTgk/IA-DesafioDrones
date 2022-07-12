using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour {

    public GameObject AgentPrefab;

    private bool isTraning = false;
    [SerializeField]
    private int populationSize = 2;
    [SerializeField]
    private float generationTime = 10f;
    public int generationNumber = 0;
    private int[] layers = new int[] {20, 30, 30, 20, 10, 5, 1}; //20 inputs and 1 output
    private List<NeuralNetwork> nets;
    private List<Agent> agentsList = null;
    

    [SerializeField]
    private bool loadOnStart;
    
    [SerializeField]
    private bool train;

    [SerializeField]
    private Map map;

    private Tournament t;

    void OnGameEnd()
    {
        //Debug.Log("Morreu Geral");
        isTraning = false;
        CreateAgentBodies();
    }

    void Timer()
    {
        isTraning = false;
    }

    private void Start()
    {
        InitAgentNeuralNetworks();
        generationNumber++;
        isTraning = true;
        CreateAgentBodies();
    }

    void FixedUpdate ()
    {
        if (isTraning == false && train)
        {

            nets.Sort();
            nets[nets.Count-1].Save(); //Saving the fittest for LoadingStart
            agentsList.Find(p => p.GetLocalBot()?.getNet() == nets[nets.Count - 1])?.SetAsFittest();
            
            for (int i = 0; i < populationSize / 2; i++)
            {
                nets[i] = new NeuralNetwork(nets[i+(populationSize / 2)]);
                nets[i].Mutate();

                nets[i + (populationSize / 2)] = new NeuralNetwork(nets[i + (populationSize / 2)]); //too lazy to write a reset neuron matrix values method....so just going to make a deep-copy lol
            }

            for (int i = 0; i < populationSize; i++)
            {
                nets[i].SetFitness(0f);
            }
            
            generationNumber++;
            isTraning = true;
            Invoke("Timer",generationTime);
        }
    }

    private void CreateAgentBodies()
    {
        if (agentsList != null)
        {
            for (int i = 0; i < agentsList.Count; i++)
            {
                agentsList[i].GetBot()?.Quit();
                Destroy(agentsList[i].gameObject);
                
            }

        }

        agentsList = new List<Agent>();
        if (train)
        {
            if(t == null)
                t = gameObject.GetComponent<Tournament>();
            map.ClearMap();
            map.ReGenerateMap();
            t.SetMap(map);
            for (int i = 0; i < populationSize; i++)
            {
                Agent agent = (Instantiate(AgentPrefab, new Vector3(UnityEngine.Random.Range(-10f,10f), UnityEngine.Random.Range(-10f, 10f), 0),AgentPrefab.transform.rotation)).GetComponent<Agent>();
                agent.Init(nets[i],i,train, t);
                agent.GetLocalBot().GameEnded += OnGameEnd;
                agentsList.Add(agent);
            }
            t.StartTournament();
            
            isTraning = false;
            
            return;
        }

        for (int i = 0; i < populationSize; i++)
        {
            Agent agent = (Instantiate(AgentPrefab, new Vector3(UnityEngine.Random.Range(-10f,10f), UnityEngine.Random.Range(-10f, 10f), 0),AgentPrefab.transform.rotation)).GetComponent<Agent>();
            agent.Init(nets[i],i,train);
            agent.GetBot().GameEnded += OnGameEnd;
            agentsList.Add(agent);
        }

    }
    
    void InitAgentNeuralNetworks()
    {
        //population must be even, just setting it to 2 incase it's not
        if (populationSize % 2 != 0 && train)
        {
            populationSize = 2;

            nets = new List<NeuralNetwork>();
        
            if (loadOnStart)
            {
                NeuralNetwork Dummynet = new NeuralNetwork(); //Thats kinda lazy to do
                for (int i = 0; i < populationSize; i++)
                {
                    var net = Dummynet.Load();
                    net.Mutate();
                    nets.Add(net);
                }
            
            }
            else
            {
                for (int i = 0; i < populationSize; i++)
                {
                    NeuralNetwork net = new NeuralNetwork(layers);
                    net.Mutate();
                    nets.Add(net);
                } 
            }
        }
        else if (train)
        {
            nets = new List<NeuralNetwork>();
        
            if (loadOnStart)
            {
                NeuralNetwork Dummynet = new NeuralNetwork(); //Thats kinda lazy to do
                for (int i = 0; i < populationSize; i++)
                {
                    var net = Dummynet.Load();
                    net.Mutate();
                    nets.Add(net);
                }
            
            }
            else
            {
                for (int i = 0; i < populationSize; i++)
                {
                    NeuralNetwork net = new NeuralNetwork(layers);
                    net.Mutate();
                    nets.Add(net);
                } 
            }
        }
        else
        {
            //If not training only one guy
            populationSize = 1;
            nets = new List<NeuralNetwork>();
            
            if (loadOnStart)
            {
                NeuralNetwork Dummynet = new NeuralNetwork(); //Thats kinda lazy to do
                for (int i = 0; i < populationSize; i++)
                {
                    var net = Dummynet.Load();
                    nets.Add(net);
                }
            
            }
            else
            {
                for (int i = 0; i < populationSize; i++)
                {
                    NeuralNetwork net = new NeuralNetwork(layers);
                    nets.Add(net);
                } 
            } 
            
        }

        
    }
}
