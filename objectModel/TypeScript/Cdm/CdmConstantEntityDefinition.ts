import {
    AttributeContextParameters,
    CdmAttributeContext,
    cdmAttributeContextType,
    CdmCorpusContext,
    CdmEntityReference,
    CdmObject,
    CdmObjectDefinitionBase,
    cdmObjectType,
    Logger,
    ResolvedAttributeSet,
    ResolvedAttributeSetBuilder,
    ResolvedTraitSetBuilder,
    resolveOptions,
    VisitCallback
} from '../internal';

export class CdmConstantEntityDefinition extends CdmObjectDefinitionBase {
    public constantEntityName: string;
    public entityShape: CdmEntityReference;
    public constantValues: string[][];

    public static get objectType(): cdmObjectType {
        return cdmObjectType.constantEntityDef;
    }

    constructor(ctx: CdmCorpusContext) {
        super(ctx);
        // let bodyCode = () =>
        {
            this.objectType = cdmObjectType.constantEntityDef;
        }
        // return p.measure(bodyCode);
    }

    public copy(resOpt?: resolveOptions, host?: CdmObject): CdmObject {
        // let bodyCode = () =>
        {
            if (!resOpt) {
                resOpt = new resolveOptions(this);
            }
            let copy: CdmConstantEntityDefinition;
            if (!host) {
                copy = new CdmConstantEntityDefinition(this.ctx);
            } else {
                copy = host as CdmConstantEntityDefinition;
                copy.ctx = this.ctx;
            }
            copy.constantEntityName = this.constantEntityName;
            copy.entityShape = <CdmEntityReference>this.entityShape.copy(resOpt);
            copy.constantValues = this.constantValues; // is a deep copy needed?
            this.copyDef(resOpt, copy);

            return copy;
        }
        // return p.measure(bodyCode);
    }

    public validate(): boolean {
        // let bodyCode = () =>
        {
            if (this.constantValues === undefined) {
                const pathSplit: string[] = this.declaredPath.split('/');
                const entityName: string = (pathSplit.length > 0) ? pathSplit[0] : ``;
                Logger.warning(this.constantEntityName, this.ctx, `constant entity '${entityName}' defined without a constant value.'`);
            }

            return this.entityShape ? true : false;
        }
        // return p.measure(bodyCode);
    }

    public getObjectType(): cdmObjectType {
        // let bodyCode = () =>
        {
            return cdmObjectType.constantEntityDef;
        }
        // return p.measure(bodyCode);
    }

    public isDerivedFrom(base: string, resOpt?: resolveOptions): boolean {
        // let bodyCode = () =>
        {
            if (!resOpt) {
                resOpt = new resolveOptions(this);
            }

            return false;
        }
        // return p.measure(bodyCode);
    }

    public getName(): string {
        // let bodyCode = () =>
        {
            return this.constantEntityName;
        }
        // return p.measure(bodyCode);
    }

    public getEntityShape(): CdmEntityReference {
        // let bodyCode = () =>
        {
            return this.entityShape;
        }
        // return p.measure(bodyCode);
    }

    public setEntityShape(shape: CdmEntityReference): CdmEntityReference {
        // let bodyCode = () =>
        {
            this.entityShape = shape;

            return this.entityShape;
        }
        // return p.measure(bodyCode);
    }

    public getConstantValues(): string[][] {
        // let bodyCode = () =>
        {
            return this.constantValues;
        }
        // return p.measure(bodyCode);
    }

    public setConstantValues(values: string[][]): string[][] {
        // let bodyCode = () =>
        {
            this.constantValues = values;

            return this.constantValues;
        }
        // return p.measure(bodyCode);
    }

    public visit(pathFrom: string, preChildren: VisitCallback, postChildren: VisitCallback): boolean {
        // let bodyCode = () =>
        {
            let path: string = '';
            if (!this.ctx.corpus.blockDeclaredPathChanges) {
                path = this.declaredPath;
                if (!path) {
                    path = pathFrom + (this.constantEntityName ? this.constantEntityName : '(unspecified)');
                    this.declaredPath = path;
                }
            }

            if (preChildren && preChildren(this, path)) {
                return false;
            }
            if (this.entityShape) {
                if (this.entityShape.visit(`${path}/entityShape/`, preChildren, postChildren)) {
                    return true;
                }
            }
            if (postChildren && postChildren(this, path)) {
                return true;
            }

            return false;
        }
        // return p.measure(bodyCode);
    }

    /**
     * @internal
     */
    public constructResolvedTraits(rtsb: ResolvedTraitSetBuilder, resOpt: resolveOptions): void {
        // let bodyCode = () =>
        {
            return;
        }
        // return p.measure(bodyCode);
    }

    /**
     * @internal
     */
    public constructResolvedAttributes(resOpt: resolveOptions, under?: CdmAttributeContext): ResolvedAttributeSetBuilder {
        // let bodyCode = () =>
        {
            const rasb: ResolvedAttributeSetBuilder = new ResolvedAttributeSetBuilder();
            let acpEnt: AttributeContextParameters;
            if (under) {
                acpEnt = {
                    under: under,
                    type: cdmAttributeContextType.entity,
                    name: this.entityShape.fetchObjectDefinitionName(),
                    regarding: this.entityShape,
                    includeTraits: true
                };
            }

            if (this.entityShape) {
                rasb.mergeAttributes(this
                    .getEntityShape()
                    .fetchResolvedAttributes(resOpt, acpEnt));
            }

            // things that need to go away
            rasb.removeRequestedAtts();

            return rasb;
        }
        // return p.measure(bodyCode);
    }

    // the world's smallest complete query processor...
    /**
     * @internal
     */
    public findValue(
        resOpt: resolveOptions,
        attReturn: string | number,
        attSearch: string | number,
        valueSearch: string,
        order: number,
        action: (found: string) => string): void {
        // let bodyCode = () =>
        {
            let resultAtt: number = -1;
            let searchAtt: number = -1;

            if (typeof (attReturn) === 'number') {
                resultAtt = attReturn;
            }
            if (typeof (attSearch) === 'number') {
                searchAtt = attSearch;
            }

            if (resultAtt === -1 || searchAtt === -1) {
                // metadata library
                const ras: ResolvedAttributeSet = this.fetchResolvedAttributes(resOpt);
                // query validation and binding
                const l: number = ras.set.length;
                for (let i: number = 0; i < l; i++) {
                    const name: string = ras.set[i].resolvedName;
                    if (resultAtt === -1 && name === attReturn) {
                        resultAtt = i;
                    }
                    if (searchAtt === -1 && name === attSearch) {
                        searchAtt = i;
                    }
                    if (resultAtt >= 0 && searchAtt >= 0) {
                        break;
                    }
                }
            }

            // rowset processing
            if (resultAtt >= 0 && searchAtt >= 0) {
                if (this.constantValues && this.constantValues.length) {
                    let startAt: number = 0;
                    let endBefore: number = this.constantValues.length;
                    let increment: number = 1;
                    if (order === -1) {
                        increment = -1;
                        startAt = this.constantValues.length - 1;
                        endBefore = -1;
                    }
                    for (let i: number = startAt; i !== endBefore; i += increment) {
                        if (this.constantValues[i][searchAtt] === valueSearch) {
                            this.constantValues[i][resultAtt] = action(this.constantValues[i][resultAtt]);

                            return;
                        }
                    }
                }
            }

            return;
        }
        // return p.measure(bodyCode);
    }

    /**
     * @internal
     */
    public fetchConstantValue(
        resOpt: resolveOptions,
        attReturn: string | number,
        attSearch: string | number,
        valueSearch: string,
        order: number): string {
        // let bodyCode = () =>
        {
            let result: string;
            this.findValue(resOpt, attReturn, attSearch, valueSearch, order, (found: string): string => {
                result = found;

                return found;
            });

            return result;
        }
        // return p.measure(bodyCode);
    }

    /**
     * @internal
     */
    public updateConstantValue(
        resOpt: resolveOptions,
        attReturn: string | number,
        newValue: string,
        attSearch: string | number,
        valueSearch: string,
        order: number): string {
        // let bodyCode = () =>
        {
            let result: string;
            this.findValue(resOpt, attReturn, attSearch, valueSearch, order, (found: string): string => {
                result = found;

                return newValue;
            });

            return result;
        }
        // return p.measure(bodyCode);
    }
}
