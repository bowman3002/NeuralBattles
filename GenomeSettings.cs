using System;

public class GenomeSettings {
    public readonly double CONNECTION_MUTATE_RATE, LINK_MUTATE_RATE, NODE_MUTATE_RATE, CROSSOVER_RATE;

    public GenomeSettings(double connection, double link, double node, double crossover) {
        CONNECTION_MUTATE_RATE = connection;
        LINK_MUTATE_RATE = link;
        NODE_MUTATE_RATE = node;
        CROSSOVER_RATE = crossover;
    }
}
