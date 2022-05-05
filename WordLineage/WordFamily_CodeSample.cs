/* Word Lineage WPF Project
 * by Jennifer Fullerton
 * WordFamily.cs
 * Standard class to contain a collection of related nodes containing information about relations to other nodes.
 * Topological sort algorithm modified from example on interviewcake https://www.interviewcake.com/concept/java/topological-sort
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WordLineage
{
	/// <summary>
    /// Creates a word family composed of WordNodes sorted in toplogical order.
    /// </summary>
    public class WordFamily
	{
        #region Fields
        /// <summary>
        /// The name of the WordFamily.
        /// </summary>
        public string Name;
        /// <summary>
        /// The list of all WordNodes in this WordFamily in topological order.
        /// </summary>
        /// <remarks>
        ///     The list contains a single copy of each word. 
        ///     The list is intended to be sorted in topological order.
        /// </remarks>
		public ObservableCollection<WordNode> T_Nodes;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new DAG to contain a family of WordNodes.
        /// </summary>
        /// <param name="name">The name of this word family. Defaults to "Word Family 1."</param>
        public WordFamily(string name = "Word Family 1")
		{
			this.Name = name;
			this.T_Nodes = new();
		}
        #endregion

        #region Methods
        /// <summary>
        /// Adds a new word to this WordFamily.
        /// </summary>
        /// <param name="word">The new word to be added to this WordFamily.</param>
        public void AddWord(WordNode word)
        {
            if (!this.T_Nodes.Contains(word))
            {
                this.T_Nodes.Add(word);
            }
        }

        /// <summary>
        /// Adds multiple words to this WordFamily.
        /// </summary>
        /// <param name="words">The list of words to be added to this WordFamily.</param>
        public void AddWords(params WordNode[] words)
        {
            foreach (WordNode word in words)
            {
                this.AddWord(word);
            }
        }

        /// <summary>
        /// Removes a word from this WordFamily, as well as any connections to it.
        /// </summary>
        /// <param name="word">The word to be removed from this WordFamily.</param>
        public void RemoveWord(WordNode word)
        {
            word.RemoveAllConnections();
            this.T_Nodes.Remove(word);
        }

        /// <summary>
        /// Removes a list of words from the WordFamily, as well as any connections to each word.
        /// </summary>
        /// <param name="words">The list of words to be removed.</param>
        public void RemoveWords(params WordNode[] words)
        {
            foreach (WordNode word in words)
            {
                RemoveWord(word);
            }
        }

        /// <summary>
        /// Sorts this family's T_Nodes into toplological order.
        /// </summary>
        /// <returns>
        /// Returns null upon succesful sort.
        /// If the sort fails, returns a list of Nodes that caused a cycle. No change is made.
        /// </returns>
        /// <remarks>
        /// Adapted from example on interviewcake https://www.interviewcake.com/concept/java/topological-sort
        /// </remarks>
        public List<WordNode>? Sort()
        {
            // Dictionary tracks the remaining indegrees for each node
            Dictionary<WordNode, int> indegrees = new(T_Nodes.Count);

            // Queue tracks nodes with indegree of zero for processing
            Queue<WordNode> nodes_with_indegree_zero = new();
            
            // collect indegree (# of parents) of each node; push nodes with indegree zero into the queue
            foreach (WordNode node in T_Nodes)
            {
                indegrees[node] = node.Parents.Count;
                if(node.Parents.Count == 0)
                {
                    nodes_with_indegree_zero.Enqueue(node);
                }
            }

            // create a new list to store the sorted nodes
            ObservableCollection<WordNode> topological_ordering = new();
            
            // begin the sorting process
            // the sort will continue until there are no more nodes left to process
            // OR if a cycle is found (in which case it will break and return problem nodes)
            while(nodes_with_indegree_zero.Count > 0)
            {
                WordNode node = nodes_with_indegree_zero.Dequeue();
                topological_ordering.Add(node);

                // go through each child of this node 
                foreach (WordNode child in node.Children)
                {
                    // decrement the number of remaining indegrees for each child of the parent node that has been removed.
                    indegrees[child]--;
                    if(indegrees[child] == 0)
                    {
                        // if any of the children have all parents removed, add them to the processing queue.
                        nodes_with_indegree_zero.Enqueue(child);
                    }
                }
            }

            // final checks!
            // if the new length of the list matches the the old length, sorting is successful
            if( topological_ordering.Count == T_Nodes.Count)
            {
                // set the old list equal to the new ordered list (return null)
                this.T_Nodes = topological_ordering;
                return null;
            } else
            {
                // if the list is not the same length, then the graph has a cycle.
                // go through the dictionary, collect each entry that has 1 or more
                // indegrees, and return the list to search through.
                List<WordNode> problem_nodes = new();

                foreach(WordNode n in T_Nodes)
                {
                    if( indegrees[n] > 0)
                    {
                        problem_nodes.Add(n);
                    }
                }
                return problem_nodes;
            }
        }
        #endregion

        #region Overrrides
        /// <summary>
        /// Returns the string Name of this WordFamily.
        /// </summary>
        /// <returns>The string Name of this WordFamily.</returns>
        public override string ToString()
        {
            return this.Name;
        }
        #endregion
    }

}