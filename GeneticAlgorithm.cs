﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticAlgorithm
{
    public class CellAutomataGA
    {
        private const int POPULATION = 100;
        private const int ELITE_POPULATION = 2;
        private const int CHROMOSOME_SIZE = 2000;
        private const int MOORE_NEIGHBORHOOD = 9;
        private const int CELL_STATE_SIZE = 8;
        private const double CROSSOVER_RATE = 1;
        private const double MUTATION_RATE = 0.2;
        private const double MUTATION_RANGE = 0.05; 
        private const double STABILITY = 1;
        private const int EPISODES_FOR_EVALATION = 50;
        private int chromosomeMaxNumber;

        private IntArrayChromosomes intArrayChromosomes;
        private double[] scores;
        private List<int> elitelist;

        public int[] rulesForEvalate;

        public CellAutomataGA()
        {
            chromosomeMaxNumber = FastPower(CELL_STATE_SIZE, MOORE_NEIGHBORHOOD + 1) - 1;
            intArrayChromosomes = new IntArrayChromosomes(POPULATION, CHROMOSOME_SIZE, chromosomeMaxNumber);
            SetUpChromosomes();
            scores = new double[POPULATION];
            elitelist = new List<int>();

            rulesForEvalate = new int[5]{
                ConvertToConditionNo(new int[] { 1, 0, 1, 0, 0, 0, 0, 0, 0, 0 }),
                ConvertToConditionNo(new int[] { 1, 1, 0, 0, 0, 0, 0, 0, 0, 0 }),
                ConvertToConditionNo(new int[] { 1, 0, 0, 0, 0, 0, 0, 1, 0, 0 }),
                ConvertToConditionNo(new int[] { 1, 0, 0, 0, 0, 0, 1, 0, 0, 0 }),
                ConvertToConditionNo(new int[] { 1, 0, 0, 0, 0, 0, 0, 0, 1, 0 }),
            };
        }

        public void SetUpChromosomes()
        {
            Random random = new Random();
            for (int i = 0; i < POPULATION; i++) {
                for(int j = 0; j < CHROMOSOME_SIZE / 20; j++) {
                    int rule = 0;
                    for(int k = 0; k < MOORE_NEIGHBORHOOD; k++) {
                        rule +=  random.Next(0, 2) * FastPower(CELL_STATE_SIZE, k);
                    }
                    rule += random.Next(0, CELL_STATE_SIZE) * FastPower(CELL_STATE_SIZE, MOORE_NEIGHBORHOOD);
                    intArrayChromosomes.SetChromosome(i, j, rule);
                }
            }
        }

        public void Crossover(IntArrayChromosomes intChromosomes, double crossoverrate, List<int> _elitelist)
        {
            Random random = new Random();
            for(int i = 0; i < POPULATION - 1; i++) {
                double rnddbl = random.NextDouble();
                int rndindex = random.Next(i, POPULATION - 1);
                if (rnddbl <= crossoverrate) {
                    if(!elitelist.Contains(i) && !elitelist.Contains(rndindex)) {
                        int point1 = random.Next(0, CHROMOSOME_SIZE);
                        int point2 = random.Next(0, CHROMOSOME_SIZE);
                        if (point1 <= point2) intArrayChromosomes.TransChromosome(i, rndindex, point1, point2);
                        else intArrayChromosomes.TransChromosome(i, rndindex, point2, point1);
                    }else if (elitelist.Contains(i) && elitelist.Contains(rndindex)) {

                    }
                    else if (elitelist.Contains(rndindex)) {
                        int[] eliterule = intChromosomes.ReadChromosomeAsRule(rndindex);
                        int point1 = random.Next(0, CHROMOSOME_SIZE);
                        int point2 = random.Next(0, CHROMOSOME_SIZE);
                        if (point1 <= point2) intArrayChromosomes.TransChromosome(i, rndindex, point1, point2);
                        else intArrayChromosomes.TransChromosome(i, rndindex, point2, point1);
                        intChromosomes.SetChromosomeAsRule(rndindex, eliterule);
                    }else if (elitelist.Contains(i)) {
                        int[] eliterule = intChromosomes.ReadChromosomeAsRule(i);
                        int point1 = random.Next(0, CHROMOSOME_SIZE);
                        int point2 = random.Next(0, CHROMOSOME_SIZE);
                        if (point1 <= point2) intArrayChromosomes.TransChromosome(i, rndindex, point1, point2);
                        else intArrayChromosomes.TransChromosome(i, rndindex, point2, point1);
                        intChromosomes.SetChromosomeAsRule(i, eliterule);
                    }
                }
            }
        }

        public void Evaluate()
        {
            for(int i = 0; i < POPULATION; i++) {
                CellAutomataGame cellAutomataGame = new CellAutomataGame(intArrayChromosomes.ReadChromosomeAsRule(i), rulesForEvalate);
                cellAutomataGame.InitializeBoards();
                for(int n = 0; n < EPISODES_FOR_EVALATION; n++) {
                    cellAutomataGame.UpdateGameBoard();
                }
                scores[i] = EvaluateFunction(cellAutomataGame);
            }
        }

        private double EvaluateFunction(CellAutomataGame cellAutomataGame)
        {
            double score1 = 0;
            double score2 = 0;
            for(int x = 0; x < cellAutomataGame.BOARD_SIZE; x++) {
                for(int y = 0; y < cellAutomataGame.BOARD_SIZE; y++) {
                    if (cellAutomataGame.board1.GetCell(cellAutomataGame.board1.board, x, y) != 0) score1++;
                    score2 += cellAutomataGame.board2.GetCell(cellAutomataGame.board2.board, x, y);
                }
            }
            return Math.Abs(score1 / Math.Pow(cellAutomataGame.BOARD_SIZE, 2) / score2);
        }

        public void Mutation(IntArrayChromosomes intChromosomes, double mutationrate, List<int> _elitelist, int chromosomeMaxNumber)
        {
            Random random = new Random();
            for(int i = 0; i < POPULATION; i++) {
                double rndnum = random.NextDouble();
                if(rndnum <= mutationrate * (1 - STABILITY * scores[i])) {
                    for (int j = 0; j < CHROMOSOME_SIZE / 10 * MUTATION_RANGE; j++) {
                        intChromosomes.SetChromosome(i, random.Next(0, CHROMOSOME_SIZE), random.Next(0, chromosomeMaxNumber));
                    }
                }
            }
        }

        public void SelectElite(int elitepopulation)
        {
            elitelist.Clear();

            double[] tmpscores = new double[POPULATION];//
            scores.CopyTo(tmpscores, 0);//
            Array.Sort(tmpscores);//sort
            Array.Reverse(tmpscores);

            double elitescore = tmpscores[elitepopulation - 1];
            int count = 0;
            for(int i = 0; i < POPULATION; i++) {
                if (scores[i] >= elitescore) {
                    elitelist.Add(i);
                    count++;
                }
                if (count >= ELITE_POPULATION) break;
            }
        }

        public void NextGeneration()
        {
            Evaluate();
            SelectElite(ELITE_POPULATION);
            Crossover(intArrayChromosomes, CROSSOVER_RATE, elitelist);
            Mutation(intArrayChromosomes, MUTATION_RATE, elitelist, chromosomeMaxNumber);
        }

        public int FastPower(int _base, int exponent)
        {
            if (exponent == 0) return 1;
            else if (exponent == 1) return _base;
            else if (exponent % 2 == 0) {
                int tmp = FastPower(_base, exponent / 2);
                return tmp * tmp;
            } else {
                int tmp = FastPower(_base, (exponent - 1) / 2);
                return tmp * tmp * _base;
            }
        }

        public int ConvertToConditionNo(int[] input)
        {
            int returnvalue = 0;
            for(int i = 0; i <= MOORE_NEIGHBORHOOD; i++) {
                returnvalue += FastPower(CELL_STATE_SIZE, i) * input[MOORE_NEIGHBORHOOD - i];
            }
            return returnvalue;
        }

        public void ShowScores()
        {
            Console.WriteLine("Scores:");
            for (int i = 0; i < POPULATION; i++) {
                Console.WriteLine(scores[i]);
            }
        }

        public void ShowEliteScores()
        {
            Console.WriteLine("EliteScores");
            foreach(int index in elitelist) {
                Console.WriteLine(index);
                Console.WriteLine(scores[index]);
            }
        }

        public int[] EliteRule()
        {
            return intArrayChromosomes.ReadChromosomeAsRule(elitelist[0]);
        }
    }
}
