FROM registry.access.redhat.com/rhscl/python-36-rhel7

USER root


#ADD rh-python36.tar.gz /opt
#RUN rpm -i /opt/rh-python36/*

#ENV PATH=/opt/rh/rh-python36/root/usr/bin:$PATH \
#    LD_LIBRARY_PATH=/opt/rh/rh-python36/root/usr/lib64:$LD_LIBRARY_PATH \
#    MANPATH=/opt/rh/rh-python36/root/usr/share/man:$MANPATH \
#    PKG_CONFIG_PATH=/opt/rh/rh-python36/root/usr/lib64/pkgconfig:$PKG_CONFIG_PATH \
#    XDG_DATA_DIRS=/opt/rh/rh-python36/root/usr/share:$XDG_DATA_DIRS

ADD pip.tar.gz /tmp
RUN pip install --no-index /tmp/pip/pip-18.1-py2.py3-none-any.whl 

CMD ["/bin/bash"]
