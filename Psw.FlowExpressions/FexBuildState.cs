// -----------------------------------------------------------------------------
// Copyright (c) 2023 Promic Software. All rights reserved.
// Licensed under the MIT License (MIT).
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Psw.FlowExpressions
{
    public class FexBuildState<T>
    {
        public FexElement<T>? HostFex;
        public FexElement<T>? LastFex;
        public FexOpr<T>? LastOpr;
        public FexPreOp<T> PreOp = new FexPreOp<T>(null);  // Will be added to every Op - use for e.g to skip spaces
        public Dictionary<string, FexRef<T>> RefSet = new Dictionary<string, FexRef<T>>();

        public FexElement<T> Production(FexBuilder<T> builder, FexElement<T> prod, Action<FexBuilder<T>> build) {
            HostFex = prod;
            build?.Invoke(builder);
            return prod;
        }

        public FexBuilder<T> AddAndBuild(FexBuilder<T> builder, Action<FexBuilder<T>> build, FexElement<T> prod, bool setLastFex = false) {
            var prevHost = HostFex;

            HostFex.Add(prod);
            HostFex = prod;
            build?.Invoke(builder);
            HostFex = prevHost;
            LastFex = setLastFex ? prod : null;
            return builder;
        }

        public void LogRef(string refName) {
            if (RefSet.ContainsKey(refName)) RefSet[refName].Ref = HostFex;
            else RefSet[refName] = new FexRef<T> { Ref = HostFex };
        }

        public void LinkRef(string refName) {
            FexRef<T> fexRef;
            if (RefSet.ContainsKey(refName)) fexRef = RefSet[refName];
            else {
                fexRef = new FexRef<T>();
                RefSet[refName] = fexRef;
            }
            HostFex.Add(fexRef);
        }
    }
}
