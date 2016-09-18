using System.Collections.Generic;
using System;

//A collection of two "chromosomes" that can be used to create a NeuralNetwork
public class Genome {
    public float r, b, g;

    private string _name;
	
    private List<List<Gene>> _genotype;
    
    private List<Gene> _phenotype;
    
    private NeuralNetwork _network;
    
    private int _fitness;

    private GenomeSettings _settings;
    
    //Creates a child Genome by creating a gamete from both Genomes
    public Genome(string i_name, Genome i_g1, Genome i_g2, GenomeSettings i_settings, NeuralSettings neuralSettings) {
        _settings = i_settings;

        _name = i_name;
        
        _genotype = new List<List<Gene>>();
        _genotype.Add(i_g1.createGamete(InvasionLearning.CROSSOVER_CHANCE));
        _genotype.Add(i_g2.createGamete(InvasionLearning.CROSSOVER_CHANCE));
        
        //A phenotype needs to exist to create a network from
        _phenotype = createPhenotype(_genotype);
        //A network needs to exist before mutation to know what can be mutated
        _network = new NeuralNetwork(this, neuralSettings);

		mutateChromosome(_genotype[0], this);
		mutateChromosome(_genotype[1], this);

        //Now that mutation has occured, the new mutated phenotype and network is created replacing the previous
		_phenotype = createPhenotype (_genotype);
		_network = new NeuralNetwork (this, neuralSettings);

		_fitness = 0;
    }
    
    //Creates Genome from pre-existing list of Genes representing chromosomes
    public Genome(string i_name, List<List<Gene>> i_genotype, GenomeSettings i_settings, NeuralSettings neuralSettings) {
        _name = i_name;

        _settings = i_settings;
        
        _genotype = i_genotype;
        
        _phenotype = createPhenotype(_genotype);
        
        _network = new NeuralNetwork(this, neuralSettings);
        
        _fitness = 0;
    }
    
    //Constructor that decodes string encoding of a Genome
    public Genome(string i_name, string sGenotype, GenomeSettings i_settings, NeuralSettings neuralSettings) {
        _name = i_name;

        _settings = i_settings;
        
		string alpha;
		string beta;
        string[] chromosomes = sGenotype.Split(':');

        _genotype = new List<List<Gene>>();
        _genotype.Add(new List<Gene>());
        _genotype.Add(new List<Gene>());

        _phenotype = new List<Gene>();

        if (chromosomes.Length == 2) {
			alpha = chromosomes [0];
			beta = chromosomes [1];
			
			String[] alphaSplit = alpha.Split (';');
			foreach (string s in alphaSplit) {
				_genotype [0].Add (new Gene (s));
			}
			
			String[] betaSplit = beta.Split (';');
			foreach (string s in betaSplit) {
				_genotype [1].Add (new Gene (s));
			}

			_phenotype = createPhenotype (_genotype);
		}
        
        _network = new NeuralNetwork(this, neuralSettings);
        
        _fitness = 0;
    }
    
    //A phenotype is the combination of two chromosomes that, through gene dominance, is displayed
    private List<Gene> createPhenotype(List<List<Gene>> geno) {
        List<Gene> alpha = geno[0];
        List<Gene> beta  = geno[1];
        
        //Final phenotype combining alpha and beta chromosomes
        List<Gene> pheno = new List<Gene>();
        
        //Index for alpha and beta
		int a = 0, b = 0;
		while (a < alpha.Count || b < beta.Count) {
			if (a < alpha.Count && (b == beta.Count || alpha [a].innovation < beta [b].innovation)) {
                //Have not finished alpha, have either finished beta or on an innovation only alpha has
				pheno.Add (alpha [a]);
					
				++a;

			} else if (b < beta.Count && (a == alpha.Count || alpha [a].innovation > beta [b].innovation)) {
                //Have not finished beta, have either finished alpha or on an innovation only beta has
				pheno.Add (beta [b]);
				
				++b;

			} else {
                //On same innovation for both chromosomes. Add the more dominant, randomly add if both the same dominance
				if (alpha [a].dominant && !beta [b].dominant) {
					pheno.Add (alpha [a]);
				} else if (beta [b].dominant && !alpha [a].dominant) { 
					pheno.Add (beta [b]);
				} else {
					pheno.Add ((TestSimpleRNG.SimpleRNG.GetUniform () < .5) ? alpha [a] : beta [b]);
				}
				++a; ++b;
			}
		}

        return pheno;
    }
    
    //Creates gamete with crossover to create half of a child of this genome
    private List<Gene> createGamete(double crossoverChance) {
        List<Gene> alpha = _genotype[0];
        List<Gene> beta = _genotype[1];
        
        //Future gamete combination of alpha and beta
        List<Gene> gamete = new List<Gene>();

		int a = 0, b = 0;
		while (a < alpha.Count || b < beta.Count) {
			if(a < alpha.Count && (b == beta.Count || alpha[a].innovation < beta[b].innovation)) {
                //Have not finished alpha, either have finished beta or on an innovation only alpha has
				gamete.Add(new Gene(alpha[a]));
				
				++a;
			} else if(b < beta.Count && (a == alpha.Count || alpha[a].innovation > beta[b].innovation)) {
                //Have not finished beta, either have finished alpha or on an innovation only beta has
				gamete.Add(new Gene(beta[b]));

				++b;
			} else {
                //On same innovation on both chromosomes. Pick which to put into gamete based on crossover chance
				gamete.Add((TestSimpleRNG.SimpleRNG.GetUniform() < crossoverChance) ? new Gene(alpha[a]) : new Gene(beta[b]));
				++a; ++b;
			}
		}
        
        return gamete;
    }
    
    //Controls which mutations to perform on the chromosome
    private void mutateChromosome(List<Gene> g, Genome genome) {
        if(TestSimpleRNG.SimpleRNG.GetUniform() < _settings.CONNECTION_MUTATE_RATE) {
           connectMutate(g);
		}

        if (TestSimpleRNG.SimpleRNG.GetUniform() < _settings.LINK_MUTATE_RATE) {
            linkMutate(g, genome);
		}

        if (TestSimpleRNG.SimpleRNG.GetUniform() < _settings.NODE_MUTATE_RATE) {
            nodeMutate(g, genome);
        }
    }
    
    //Changes the weight of an already existent connection by up to 100% its current weight
    private void connectMutate(List<Gene> g) {
		if(g.Count == 0) return ;

        Random r = new Random();
        Gene toChange = g[r.Next(g.Count)];
        
        if(r.Next (2) < 1) {
            toChange.weight = (toChange.weight + toChange.weight * (r.NextDouble() * 2 - 1.0));
        } else {
            toChange.weight = (toChange.weight - toChange.weight * (r.NextDouble() * 2 - 1.0));
        }
    }
    
    //Links two previously unlinked nodes
    private void linkMutate(List<Gene> g, Genome genome) {
        int n1 = genome.network.randomNeuron(false);
        int n2 = genome.network.randomNeuron(true);
        
        Random r = new Random();

        //Ensures that a neuron is not linked to itself thus avoiding infinite recursion later
        if (n1 == n2) return;

        g.Add(new Gene(n1, n2, InvasionLearning.nextInnovation(), (r.NextDouble() * 5) - 2.5));
    }
    
    //Adds a new node between an already existent connection
    private void nodeMutate(List<Gene> g, Genome genome) {
		if (g.Count == 0) {
			return;
		}

        Random r = new Random();
        
        Gene gene = g[r.Next(g.Count)];
        
        Gene newGene = new Gene(gene.inNode, genome.network.nextNeuron(), InvasionLearning.nextInnovation(), 1.0);
        gene.inNode = newGene.outNode;
        
        g.Add(newGene);
    }

    public string name {
        get {
            return _name;
        }
    }

    public NeuralNetwork network {
        get {
            return _network;
        }
    }

    public int fitness {
        get {
            return _fitness;
        }
        set {
            _fitness = value;
        }
    }
    
    public List<Gene> phenotype {
        get {
            return _phenotype;
        }
    }

    //Encodes this genome into a string that can be decoded by the string constructor
    override public string ToString() {
        string r = "";

        foreach (Gene g in _genotype[0]) {
            r += g.ToString();
        }

        r += ":";

        foreach (Gene g in _genotype[1]) {
            r += g.ToString();
        }

        return r;
    }

    public PreGenome toPreGenome() {
        return new PreGenome(name, ToString(), r, g, b);
    }
}
