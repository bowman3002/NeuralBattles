using UnityEngine;
using System.Collections;
using System;

//Serializable class that stores the same data as a Genome
//Used in the Unity editor to create genomes that are turned into real Genomes at runtime
[Serializable]
public class PreGenome {
	public string name;
	public string chromosome;

	public float r, g, b;

    //Decodes string s created by ToString into a PreGenome
    public PreGenome(string s) {
        string[] split = s.Split('|');

        name = split[0];
        chromosome = split[1];

        r = Convert.ToInt32(split[2]);
        g = Convert.ToInt32(split[3]);
        b = Convert.ToInt32(split[4]);
    }

    public PreGenome(string _name, string _chromosomes, float _r, float _g, float _b) {
        name = _name;
        chromosome = _chromosomes;
        r = _r;
        g = _g;
        b = _b;
    }

	public Genome generateGenome(GenomeSettings genomeSettings, NeuralSettings neuralSettings) {
		Genome ge = new Genome (name, chromosome, genomeSettings, neuralSettings);
		ge.r = r;
		ge.g = g;
		ge.b = b;

		return ge;
	}

    override public string ToString() {
        return name + "|" + chromosome + "|" + r + "|" + g + "|" + b;
    }
}
