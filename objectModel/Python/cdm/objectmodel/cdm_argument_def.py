from typing import Optional, TYPE_CHECKING
import json

from cdm.enums import CdmObjectType
from cdm.utilities import ResolveOptions

from .cdm_object import CdmObject
from .cdm_object_simple import CdmObjectSimple

if TYPE_CHECKING:
    from cdm.objectmodel import CdmArgumentValue, CdmCorpusContext, CdmParameterDefinition
    from cdm.utilities import FriendlyFormatNode, VisitCallback


class CdmArgumentDefinition(CdmObjectSimple):
    def __init__(self, ctx: 'CdmCorpusContext', name: str) -> None:
        super().__init__(ctx)

        # the argument explanation.
        self.explanation = None  # type: Optional[str]

        # the argument name.
        self.name = name  # type: str

        # the argument value.
        self.value = None  # type: Optional[CdmArgumentValue]

        self.resolved_parameter = None

        # Internal

        self._declared_path = None  # Optional[str]
        self._unresolved_value = None  # type: Optional[CdmArgumentValue]

    @property
    def object_type(self) -> CdmObjectType:
        return CdmObjectType.ARGUMENT_DEF

    def copy(self, res_opt: Optional['ResolveOptions'] = None, host: Optional['CdmArgumentDefinition'] = None) -> 'CdmArgumentDefinition':
        res_opt = res_opt if res_opt is not None else ResolveOptions(wrt_doc=self)

        if not host:
            copy = CdmArgumentDefinition(self.ctx, self.name)
        else:
            copy = host
            copy.ctx = self.ctx
            copy.name = self.name

        if self.value:
            if isinstance(self.value, CdmObject):
                copy.value = self.value.copy(res_opt)
            elif isinstance(self.value, object):
                # TODO: check if the type check should be dict
                copy.value = dict(self.value)
            else:
                copy.value = self.value

        copy.resolved_parameter = self.resolved_parameter
        copy.explanation = self.explanation
        return copy

    def get_name(self) -> str:
        return self.name

    def validate(self) -> bool:
        return bool(self.value)

    def visit(self, path_from: str, pre_children: 'VisitCallback', post_children: 'VisitCallback') -> bool:
        path = ''
        if self.ctx.corpus.block_declared_path_changes is False:
            path = self._declared_path
            if not path:
                path = '{}{}'.format(path_from, ('value/' if self.value else ''))
                self._declared_path = path

        if pre_children and pre_children(self, path):
            return False

        if self.value:
            if isinstance(self.value, CdmObject):
                if self.value.visit(path, pre_children, post_children):
                    return True

        if post_children and post_children(self, path):
            return True

        return False
