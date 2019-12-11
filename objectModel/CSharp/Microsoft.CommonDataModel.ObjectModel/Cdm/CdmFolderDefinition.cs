﻿//-----------------------------------------------------------------------
// <copyright file="CdmFolderDefinition.cs" company="Microsoft">
//      All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.CommonDataModel.ObjectModel.Cdm
{
    using Microsoft.CommonDataModel.ObjectModel.Enums;
    using Microsoft.CommonDataModel.ObjectModel.Persistence;
    using Microsoft.CommonDataModel.ObjectModel.Persistence.CdmFolder;
    using Microsoft.CommonDataModel.ObjectModel.Persistence.CdmFolder.Types;
    using Microsoft.CommonDataModel.ObjectModel.ResolvedModel;
    using Microsoft.CommonDataModel.ObjectModel.Storage;
    using Microsoft.CommonDataModel.ObjectModel.Utilities;
    using Microsoft.CommonDataModel.ObjectModel.Utilities.Logging;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// The object model implementation for Folder object.
    /// </summary>
    public class CdmFolderDefinition : CdmObjectDefinitionBase, CdmContainerDefinition
    {
        /// <summary>
        /// Mapping from document name to document implementation class.
        /// </summary>
        internal readonly IDictionary<string, CdmDocumentDefinition> DocumentLookup;

        /// <summary>
        /// Initializes a new instance of the <see cref="CdmFolderDefinition"/> class.
        /// </summary>
        /// <param name="ctx">The context.</param>
        /// <param name="name">The name.</param>
        public CdmFolderDefinition(CdmCorpusContext ctx, string name)
            : base(ctx)
        {
            this.Name = name;
            this.FolderPath = name + "/";
            this.ChildFolders = new CdmFolderCollection(ctx, this);
            this.Documents = new CdmDocumentCollection(ctx, this);
            this.DocumentLookup = new Dictionary<string, CdmDocumentDefinition>();
            this.ObjectType = CdmObjectType.FolderDef;
        }

        /// <summary>
        /// Gets or sets the corpus object this folder is a part of.
        /// </summary>
        internal CdmCorpusDefinition Corpus { get; set; }

        /// <summary>
        /// Gets or sets the folder name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the corpus path for the folder.
        /// </summary>
        [Obsolete("Only for internal use")]
        public string FolderPath { get; set; }

        /// <summary>
        /// Gets or sets the direct children for the directory folder.
        /// </summary>
        public CdmFolderCollection ChildFolders { get; set; }

        /// <summary>
        /// Gets or sets the child documents for the directory folder.
        /// </summary>
        public CdmDocumentCollection Documents { get; set; }

        /// <inheritdoc />
        [Obsolete("Only for internal use")]
        public string Namespace { get; set; }

        /// <summary>
        /// Creates a folder instance from data.
        /// </summary>
        /// <param name="ctx">The context.</param>
        /// <param name="obj">The object to read data from.</param>
        /// <returns>The <see cref="CdmFolderDefinition"/>.</returns>
        [Obsolete("InstanceFromData is deprecated. Please use the Persistence Layer instead.")]
        public static CdmFolderDefinition InstanceFromData(CdmCorpusContext ctx, Folder obj)
        {
            return CdmObjectBase.InstanceFromData<CdmFolderDefinition, Folder>(ctx, obj);
        }

        /// <inheritdoc />
        public override string GetName()
        {
            return this.Name;
        }

        /// <inheritdoc />
        public override bool Validate()
        {
            return !string.IsNullOrEmpty(this.Name);
        }

        /// <inheritdoc />
        public override bool IsDerivedFrom(string baseDef, ResolveOptions resOpt = null)
        {
            if (resOpt == null)
            {
                resOpt = new ResolveOptions(this);
            }

            return false;
        }

        /// <inheritdoc />
        public override string AtCorpusPath
        {
            get
            {
                if (this.Namespace == null)
                {
                    // We're not under any adapter (not in a corpus), so return special indicator.                    
                    return $"NULL:{this.FolderPath}";
                }
                else
                {
                    return $"{this.Namespace}:{this.FolderPath}";
                }
            }
        }

        [Obsolete("CopyData is deprecated. Please use the Persistence Layer instead.")]
        public override dynamic CopyData(ResolveOptions resOpt, CopyOptions options)
        {
            return CdmObjectBase.CopyData<CdmFolderDefinition>(this, resOpt, options);
        }

        /// <inheritdoc />
        public override CdmObject Copy(ResolveOptions resOpt = null, CdmObject host = null)
        {
            if (resOpt == null)
            {
                resOpt = new ResolveOptions(this);
            }

            return null;
        }


        /// <summary>
        /// Fetches the document from the folder path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="adapter">The storage adapter where the document can be found.</param>
        /// <param name="forceReload">If true, reload the object from file and replace the current object with it.</param>
        /// <returns>The <see cref="CdmDocumentDefinition"/>.</returns>
        internal async Task<CdmDocumentDefinition> FetchDocumentFromFolderPathAsync(string objectPath, StorageAdapter adapter, bool forceReload = false)
        {
            string docName;
            string remainingPath;
            int first = objectPath.IndexOf('/', 0);
            if (first < 0)
            {
                remainingPath = "";
                docName = objectPath;
            }
            else
            {
                remainingPath = objectPath.Slice(first + 1);
                docName = objectPath.Substring(0, first);
            }

            // got that doc?
            CdmDocumentDefinition doc = null;
            if (this.DocumentLookup.ContainsKey(docName))
            {
                doc = this.DocumentLookup[docName];
                if (!forceReload)
                {
                    return doc;
                }
                // remove them from the caches since they will be back in a moment
                if ((doc as CdmDocumentDefinition).IsDirty)
                {
                    Logger.Warning(nameof(CdmFolderDefinition), this.Ctx, $"discarding changes in document: {doc.Name}");
                }
                this.Documents.Remove(docName);
            }

            // go get the doc
            doc = await PersistenceLayer.LoadDocumentFromPathAsync(this, docName, doc);

            return doc;
        }

        /// <inheritdoc />
        [Obsolete]
        public override CdmObjectType GetObjectType()
        {
            return CdmObjectType.FolderDef;
        }

        /// <inheritdoc />
        public override T FetchObjectDefinition<T>(ResolveOptions resOpt = null)
        {
            if (resOpt == null)
            {
                resOpt = new ResolveOptions(this);
            }

            return default(T);
        }

        /// <inheritdoc />
        internal override ResolvedTraitSet FetchResolvedTraits(ResolveOptions resOpt = null)
        {
            if (resOpt == null)
            {
                resOpt = new ResolveOptions(this);
            }

            return null;
        }

        /// <summary>
        /// Fetches the child folder from the corpus path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="adapter">The storage adapter where the folder can be found.</param>
        /// <param name="makeFolder">Create the folder if it doesn't exist.</param>
        /// <returns>The <see cref="CdmFolderDefinition"/>.</returns>
        internal async Task<CdmFolderDefinition> FetchChildFolderFromPathAsync(string path, bool makeFolder = false)
        {
            string name;
            string remainingPath;
            int first = path.IndexOf('/', 0);
            if (first < 0)
            {
                name = path;
                remainingPath = "";
            }
            else
            {
                name = StringUtils.Slice(path, 0, first);
                remainingPath = StringUtils.Slice(path, first + 1);
            }

            if (name.ToLowerInvariant() == this.Name.ToLowerInvariant())
            {
                // the end?
                if (remainingPath.Length == 0)
                    return this;
                // check children folders
                CdmFolderDefinition result = null;
                if (this.ChildFolders != null)
                {
                    foreach (var f in this.ChildFolders)
                    {
                        result = await f.FetchChildFolderFromPathAsync(remainingPath, makeFolder);
                        if (result != null)
                            break;
                    }
                }
                if (result != null)
                    return result;

                // get the next folder
                first = remainingPath.IndexOf("/");
                name = first > 0 ? remainingPath.Slice(0, first) : remainingPath;

                if (first != -1)
                {
                    var childPath = await this.ChildFolders.Add(name).FetchChildFolderFromPathAsync(remainingPath, makeFolder);
                    if (childPath == null)
                    {
                        Logger.Error(nameof(CdmFolderDefinition), (ResolveContext)this.Ctx, $"Invalid path '{path}'", "FetchChildFolderFromPathAsync");
                    }
                    return childPath;
                }

                if (makeFolder)
                {
                    // huh, well need to make the fold here
                    return await this.ChildFolders.Add(name).FetchChildFolderFromPathAsync(remainingPath, makeFolder);
                }
                else
                {
                    // good enough, return where we got to
                    return this;
                }
            }

            return null;
        }

        /// <inheritdoc />
        public override bool Visit(string path, VisitCallback preChildren, VisitCallback postChildren)
        {
            return false;
        }
    }
}
