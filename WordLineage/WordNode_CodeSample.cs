/* Word Lineage WPF Project
 * by Jennifer Fullerton
 * WordNode.cs
 * Specialized node class that contains a word and it's relations to other words. Contains basic methods to make sure nodes
 * are added and deleted properly.
 */

using System;
using System.Collections.Generic;

namespace WordLineage
{
    /// <summary>
    /// WordNode class used to store a single word and its relations to other nodes.
    /// Intended to be used by the WordFamily class.
    /// </summary>
    public class WordNode
    {
        #region Fields
        /// <summary>
        /// The name of the word contained by this WordNode.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// An description of the word contained by this WordNode.
        /// </summary>
        public List<WordNode> Parents { get; private set; }
        /// <summary>
        /// A list of child WordNodes for this WordNode.
        /// </summary>
        public List<WordNode> Children { get; private set; }
        #endregion

        #region Constructors
        public WordNode(string name)
        {
            this.Name = name;
            this.Children = new List<WordNode>();
            this.Parents = new List<WordNode>();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds a this node as a parent and the child node as a child to the child
        /// and parent nodes, respectively. If the connection already exists between 
        /// nodes or the connection would be to itself, then no change is made.
        /// </summary>
        /// <param name="child">The second node to make a connection with.</param>
        public void AddConnection(WordNode child)
        {
            // ensure no duplicate or self-connections are made
            if (this != child && !this.Children.Contains(child) && !child.Parents.Contains(this))
            {
                // add the relationship to both nodes
                this.Children.Add(child);
                child.Parents.Add(this);
            }
        }

        /// <summary>
        /// Adds multiple directed connections between this parent WordNode and multiple child nodes.
        /// </summary>
        /// <param name="children">WordNodes that will be added as
        /// a child of this node.</param>
        public void AddConnections(params WordNode[] children)
        {
            foreach (WordNode child in children)
            {
                this.AddConnection(child);
            }
        }

        /// <summary>
        /// Removes a connection between this node and its child node. It will delete
        /// connections even if only one of the nodes contains the connection.
        /// </summary>
        /// <param name="child">The second node to remove connections between.</param>
        public void RemoveConnection(WordNode child)
        {
            // check that child is in this parent's list of children, then remove it.
            if (this.Children.Contains(child))
            {
                this.Children.Remove(child);
            }
            // check that this parent is in child's list of parents, then remove it.
            if (child.Parents.Contains(this))
            {
                child.Parents.Remove(this);
            }
        }

        /// <summary>
        /// Removes all connections from this WordNode.
        /// </summary>
        public void RemoveAllConnections()
        {
            while (this.Children.Count > 0)
            {
                this.RemoveConnection(this.Children[0]);
            }

            while (this.Parents.Count > 0)
            {
                this.Parents[0].RemoveConnection(this);
            }
        }

        #endregion

        #region Overrides
        /// <summary>
        /// String override for WordNode which returns its name.
        /// </summary>
        /// <returns>Returns the name of this WordNode.</returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion
    }
}