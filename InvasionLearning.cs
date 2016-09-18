
//Stores constants for the InvasionLearning game
public static class InvasionLearning {
    public static int currentInnovation = 0;

    public const float BLUE_SHELL = 1.001f;

    public const int INPUT_NUM = 9;
    public const int OUTPUT_NUM = 14;
    public const int MAX_NODES = 10000;
    public static NeuralSettings NEURAL_SETTINGS = new NeuralSettings(INPUT_NUM, OUTPUT_NUM, MAX_NODES);
    
    public const double CONNECTION_MUTATION_RATE = 0.5;
    public const double LINK_MUTATION_RATE =       .25;
    public const double NODE_MUTATION_RATE =       0.10;
    public const double CROSSOVER_CHANCE =         .5;
    public static GenomeSettings GENOME_SETTINGS = new GenomeSettings(CONNECTION_MUTATION_RATE, LINK_MUTATION_RATE,
                                                                      NODE_MUTATION_RATE, CROSSOVER_CHANCE);
    
    public static int nextInnovation() {
        return InvasionLearning.currentInnovation++;
    }
    
}
