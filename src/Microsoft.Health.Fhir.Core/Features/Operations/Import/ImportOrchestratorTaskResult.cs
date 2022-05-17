﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.Health.Fhir.Core.Features.Operations.Import
{
    public class ImportOrchestratorTaskResult
    {
        /// <summary>
        /// Transaction time for import task created
        /// </summary>
        public DateTimeOffset TransactionTime { get; set; }

        /// <summary>
        /// Request Uri for the import opearion
        /// </summary>
        public string Request { get; set; }

        /// <summary>
        /// Import total file size
        /// </summary>
        public long? TotalSizeInBytes { get; set; }

        /// <summary>
        /// Resource count succeed to import
        /// </summary>
        public long SucceedImportCount { get; set; }

        /// <summary>
        /// Resource count failed to import
        /// </summary>
        public long FailedImportCount { get; set; }

        /// <summary>
        /// Created task count for all blob files
        /// </summary>
        public int CreatedTaskCount { get; set; }

        /// <summary>
        /// Current end sequence id
        /// </summary>
        public long CurrentSequenceId { get; set; }

        /// <summary>
        /// Current running task id list
        /// </summary>
#pragma warning disable CA1002 // Do not expose generic lists
        public List<long> RunningTaskIds { get; } = new List<long>();
#pragma warning restore CA1002 // Do not expose generic lists

        /// <summary>
        /// Orchestrator task progress.
        /// </summary>
        public ImportOrchestratorTaskProgress Progress { get; set; }
    }
}